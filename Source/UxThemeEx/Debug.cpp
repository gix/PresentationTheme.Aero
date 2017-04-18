#include "Debug.h"

#include "BorderFill.h"
#include "Global.h"
#include "Handle.h"
#include "ImageFile.h"
#include "Utils.h"
#include "UxThemeFile.h"
#include "UxThemeHelpers.h"

#include <array>
#include <unordered_set>

#include <strsafe.h>
#include <winnt.h>
#include <winternl.h>
#include <ntstatus.h>

namespace uxtheme
{

static std::unordered_set<ENTRYHDR const*> entryHeaders;
static std::unordered_set<PARTJUMPTABLEHDR const*> partJumpTableHeaders;
static std::unordered_set<STATEJUMPTABLEHDR const*> stateJumpTableHeaders;
static std::unordered_set<PARTOBJHDR const*> partObjHeaders;
static std::unordered_set<CDrawBase const*> drawObjs;
static std::unordered_set<CTextDraw const*> textObjs;

static void FailFast()
{
    RaiseFailFastException(nullptr, nullptr, 0);
}

template<typename Container, typename T>
static bool contains(Container& c, T const& value)
{
    //return true;
    return c.find(value) != std::end(c);
}

void ClearDebugInfo()
{
    entryHeaders.clear();
    partJumpTableHeaders.clear();
    stateJumpTableHeaders.clear();
    partObjHeaders.clear();
    drawObjs.clear();
    textObjs.clear();
}

void RegisterPtr(ENTRYHDR const* hdr)
{
    entryHeaders.insert(hdr);
}

void RegisterPtr(PARTJUMPTABLEHDR const* hdr)
{
    partJumpTableHeaders.insert(hdr);
}

void RegisterPtr(STATEJUMPTABLEHDR const* hdr)
{
    stateJumpTableHeaders.insert(hdr);
}

void RegisterPtr(PARTOBJHDR const* hdr)
{
    partObjHeaders.insert(hdr);
}

void RegisterPtr(CDrawBase const* obj)
{
    drawObjs.insert(obj);
}

void RegisterPtr(CTextDraw const* obj)
{
    textObjs.insert(obj);
}

void ValidatePtr(ENTRYHDR const* hdr)
{
    if (!contains(entryHeaders, hdr))
        FailFast();
}

void ValidatePtr(PARTJUMPTABLEHDR const* hdr)
{
    if (!contains(partJumpTableHeaders, hdr))
        FailFast();
}

void ValidatePtr(STATEJUMPTABLEHDR const* hdr)
{
    if (!contains(stateJumpTableHeaders, hdr))
        FailFast();
}

void ValidatePtr(PARTOBJHDR const* hdr)
{
    if (!contains(partObjHeaders, hdr))
        FailFast();
}

void ValidatePtr(CDrawBase const* obj)
{
    if (!contains(drawObjs, obj))
        FailFast();
}

void ValidatePtr(CTextDraw const* obj)
{
    if (!contains(textObjs, obj))
        FailFast();
}

static void GetStrings(THEMEHDR const* hdr, std::vector<std::wstring>& strings)
{
    auto begin = (wchar_t const*)Advance(hdr, hdr->iStringsOffset);
    auto end = Advance(begin, hdr->iStringsLength);
    for (auto ptr = begin; ptr < end;) {
        strings.emplace_back(ptr);
        ptr += wcslen(ptr);
        while (ptr < end && *ptr == 0)
            ++ptr;
    }
}

template<typename T, typename = std::enable_if_t<std::is_enum_v<T>, T>>
static int FormatImpl(char* buffer, size_t bufferSize, T const& value)
{
    return sprintf_s(buffer, bufferSize, "%d", value);
}

template<typename T>
static int Format(char* buffer, size_t bufferSize, T const& value)
{
    return FormatImpl(buffer, bufferSize, value);
}

template<>
int Format(char* buffer, size_t bufferSize, void* const& value)
{
    return sprintf_s(buffer, bufferSize, "%p", value);
}

template<>
int Format(char* buffer, size_t bufferSize, void const* const& value)
{
    return sprintf_s(buffer, bufferSize, "%p", value);
}

template<>
int Format(char* buffer, size_t bufferSize, char const& value)
{
    return sprintf_s(buffer, bufferSize, "%d", (int)value);
}

template<>
int Format(char* buffer, size_t bufferSize, unsigned char const& value)
{
    return sprintf_s(buffer, bufferSize, "%u", (unsigned)value);
}

template<>
int Format(char* buffer, size_t bufferSize, short const& value)
{
    return sprintf_s(buffer, bufferSize, "%d", value);
}

template<>
int Format(char* buffer, size_t bufferSize, unsigned short const& value)
{
    return sprintf_s(buffer, bufferSize, "%u", value);
}

template<>
int Format(char* buffer, size_t bufferSize, int const& value)
{
    return sprintf_s(buffer, bufferSize, "%d", value);
}

template<>
int Format(char* buffer, size_t bufferSize, unsigned const& value)
{
    return sprintf_s(buffer, bufferSize, "%u", value);
}

template<>
int Format(char* buffer, size_t bufferSize, long const& value)
{
    return sprintf_s(buffer, bufferSize, "%ld", value);
}

template<>
int Format(char* buffer, size_t bufferSize, unsigned long const& value)
{
    return sprintf_s(buffer, bufferSize, "%lu", value);
}

int Format(char* buffer, size_t bufferSize, char const* value)
{
    return sprintf_s(buffer, bufferSize, "%s", value);
}

int Format(char* buffer, size_t bufferSize, wchar_t const* value)
{
    return sprintf_s(buffer, bufferSize, "%ls", value);
}

template<>
int Format(char* buffer, size_t bufferSize, POINT const& value)
{
    return sprintf_s(buffer, bufferSize, "(%ld,%ld)", value.x, value.y);
}

template<>
int Format(char* buffer, size_t bufferSize, SIZE const& value)
{
    return sprintf_s(buffer, bufferSize, "(%ld,%ld)", value.cx, value.cy);
}

template<>
int Format(char* buffer, size_t bufferSize, RECT const& value)
{
    return sprintf_s(buffer, bufferSize, "(%ld,%ld,%ld,%ld)",
                     value.left, value.top, value.right, value.bottom);
}

template<>
int Format(char* buffer, size_t bufferSize, MARGINS const& value)
{
    return sprintf_s(buffer, bufferSize, "(l:%d,r:%d,t:%d,b:%d)",
                     value.cxLeftWidth, value.cxRightWidth, value.cyTopHeight, value.cyBottomHeight);
}

template<>
int Format(char* buffer, size_t bufferSize, FILETIME const& value)
{
    return sprintf_s(buffer, bufferSize, "0x%llX",
                     ((DWORD64)value.dwLowDateTime << 32) | value.dwLowDateTime);
}

class LogFile
{
public:
    LogFile(wchar_t const* path) : path(path) {}

    bool Open()
    {
        FileHandle h{CreateFileW(path.c_str(), GENERIC_WRITE, 0, nullptr,
                                 CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr)};
        if (!h)
            return false;
        hFile = std::move(h);
        return true;
    }

    void Indent(int n = 1) { indentLevel += n; }
    void Outdent(int n = 1) { indentLevel -= n; }

    template<typename... T>
    void Log(char const* format, T const&... args)
    {
        int len = sprintf_s(buffer, countof(buffer), format, args...);
        if (len <= 0)
            return;

        WriteIndent();
        DWORD written;
        WriteFile(hFile, buffer, static_cast<DWORD>(len), &written, nullptr);
    }

    template<typename T>
    void LogPair(char const* key, T const& value)
    {
        WriteIndent();

        DWORD written;

        int len = sprintf_s(buffer, countof(buffer), "%s: ", key);
        if (len > 0)
            WriteFile(hFile, buffer, static_cast<DWORD>(len), &written, nullptr);

        len = Format(buffer, countof(buffer), value);
        if (len > 0)
            WriteFile(hFile, buffer, static_cast<DWORD>(len), &written, nullptr);

        buffer[0] = '\n';
        WriteFile(hFile, buffer, 1, &written, nullptr);
    }

    template<typename T, typename U>
    void LogPair(char const* key, T const& value, U const& value2)
    {
        WriteIndent();

        DWORD written;

        int len = sprintf_s(buffer, countof(buffer), "%s: ", key);
        if (len > 0)
            WriteFile(hFile, buffer, static_cast<DWORD>(len), &written, nullptr);

        len = Format(buffer, countof(buffer), value);
        if (len > 0)
            WriteFile(hFile, buffer, static_cast<DWORD>(len), &written, nullptr);

        buffer[0] = ' ';
        buffer[1] = '(';
        WriteFile(hFile, buffer, 2, &written, nullptr);

        len = Format(buffer, countof(buffer), value2);
        if (len > 0)
            WriteFile(hFile, buffer, static_cast<DWORD>(len), &written, nullptr);

        buffer[0] = ')';
        buffer[1] = '\n';
        WriteFile(hFile, buffer, 2, &written, nullptr);
    }

    void LogPair(char const* key, DIBINFO const& value)
    {
        Log("%s:\n", key);
        Indent();

        LogPair("uhbm.hBitmap64", value.uhbm.hBitmap64);
        LogPair("iDibOffset", value.iDibOffset);
        LogPair("iSingleWidth", value.iSingleWidth);
        LogPair("iSingleHeight", value.iSingleHeight);
        LogPair("iRgnListOffset", value.iRgnListOffset);
        LogPair("eSizingType", value.eSizingType);
        LogPair("fBorderOnly", value.fBorderOnly);
        LogPair("fPartiallyTransparent", value.fPartiallyTransparent);
        LogPair("iAlphaThreshold", value.iAlphaThreshold);
        LogPair("iMinDpi", value.iMinDpi);
        LogPair("szMinSize", value.szMinSize);

        Outdent();
    }

    template<size_t N>
    void LogPair(char const* key, wchar_t const (&value)[N])
    {
        LogPair(key, (wchar_t const*)value);
    }

    template<typename T, size_t N>
    void LogPair(char const* key, T const (&value)[N])
    {
        DWORD written;

        for (size_t i = 0; i < N; ++i) {
            WriteIndent();

            int len = sprintf_s(buffer, countof(buffer), "%s[%zu]: ", key, i);
            if (len > 0)
                WriteFile(hFile, buffer, static_cast<DWORD>(len), &written, nullptr);

            len = Format(buffer, countof(buffer), value[i]);
            if (len > 0)
                WriteFile(hFile, buffer, static_cast<DWORD>(len), &written, nullptr);

            buffer[0] = '\n';
            WriteFile(hFile, buffer, 1, &written, nullptr);
        }
    }

private:
    void WriteIndent()
    {
        if (indentLevel == 0)
            return;

        size_t length = std::min(indentLevel * (size_t)2, indentBuffer.size() - 1);
        memset(indentBuffer.data(), ' ', length);

        DWORD written;
        WriteFile(hFile, indentBuffer.data(), static_cast<DWORD>(length), &written, nullptr);
    }

    std::wstring path;
    FileHandle hFile;
    char buffer[0x800];
    std::array<char, 64> indentBuffer;
    int indentLevel = 0;
};

static void DumpDrawObj(LogFile& log, THEMEHDR const* hdr, CDrawBase* drawObj)
{
    if (drawObj->_eBgType == BT_IMAGEFILE) {
        auto imageFile = (CImageFile*)drawObj;
        log.LogPair("_eBgType", imageFile->_eBgType);
        log.LogPair("_iUnused", imageFile->_iUnused);
        log.LogPair("_ImageInfo", imageFile->_ImageInfo);
        log.LogPair("_ScaledImageInfo", imageFile->_ScaledImageInfo);
        log.LogPair("_iMultiImageCount", imageFile->_iMultiImageCount);
        log.LogPair("_eImageSelectType", imageFile->_eImageSelectType);
        log.LogPair("_iImageCount", imageFile->_iImageCount);
        log.LogPair("_eImageLayout", imageFile->_eImageLayout);
        log.LogPair("_fMirrorImage", imageFile->_fMirrorImage);
        log.LogPair("_eTrueSizeScalingType", imageFile->_eTrueSizeScalingType);
        log.LogPair("_eHAlign", imageFile->_eHAlign);
        log.LogPair("_eVAlign", imageFile->_eVAlign);
        log.LogPair("_fBgFill", imageFile->_fBgFill);
        log.LogPair("_crFill", imageFile->_crFill);
        log.LogPair("_iTrueSizeStretchMark", imageFile->_iTrueSizeStretchMark);
        log.LogPair("_fUniformSizing", imageFile->_fUniformSizing);
        log.LogPair("_fIntegralSizing", imageFile->_fIntegralSizing);
        log.LogPair("_SizingMargins", imageFile->_SizingMargins);
        log.LogPair("_ContentMargins", imageFile->_ContentMargins);
        log.LogPair("_fSourceGrow", imageFile->_fSourceGrow);
        log.LogPair("_fSourceShrink", imageFile->_fSourceShrink);
        log.LogPair("_fGlyphOnly", imageFile->_fGlyphOnly);
        log.LogPair("_eGlyphType", imageFile->_eGlyphType);
        log.LogPair("_crGlyphTextColor", imageFile->_crGlyphTextColor);
        log.LogPair("_iGlyphFontIndex", imageFile->_iGlyphFontIndex);
        log.LogPair("_iGlyphIndex", imageFile->_iGlyphIndex);
        log.LogPair("_GlyphInfo", imageFile->_GlyphInfo);
        log.LogPair("_iSourcePartId", imageFile->_iSourcePartId);
        log.LogPair("_iSourceStateId", imageFile->_iSourceStateId);
    } else if (drawObj->_eBgType == BT_BORDERFILL) {
        auto borderFill = (CBorderFill*)drawObj;
        log.LogPair("_eBgType", borderFill->_eBgType);
        log.LogPair("_iUnused", borderFill->_iUnused);
        log.LogPair("_fNoDraw", borderFill->_fNoDraw);
        log.LogPair("_eBorderType", borderFill->_eBorderType);
        log.LogPair("_crBorder", borderFill->_crBorder);
        log.LogPair("_iBorderSize", borderFill->_iBorderSize);
        log.LogPair("_iRoundCornerWidth", borderFill->_iRoundCornerWidth);
        log.LogPair("_iRoundCornerHeight", borderFill->_iRoundCornerHeight);
        log.LogPair("_eFillType", borderFill->_eFillType);
        log.LogPair("_crFill", borderFill->_crFill);
        log.LogPair("_iDibOffset", borderFill->_iDibOffset);
        log.LogPair("_ContentMargins", borderFill->_ContentMargins);
        log.LogPair("_iGradientPartCount", borderFill->_iGradientPartCount);
        log.LogPair("_crGradientColors", borderFill->_crGradientColors);
        log.LogPair("_iGradientRatios", borderFill->_iGradientRatios);
        log.LogPair("_iSourcePartId", borderFill->_iSourcePartId);
        log.LogPair("_iSourceStateId", borderFill->_iSourceStateId);
    } else {
        log.LogPair("_eBgType", drawObj->_eBgType);
        log.LogPair("_iUnused", drawObj->_iUnused);
    }
}

static void DumpEntry(LogFile& log, THEMEHDR const* hdr, ENTRYHDR* entry)
{
    log.Log("  (%05u %05u) %05u\n", entry->usTypeNum, entry->ePrimVal, entry->dwDataLen);

    log.Indent();

    if (entry->usTypeNum == TMT_PARTJUMPTBL) {
        auto jumpTableHdr = (PARTJUMPTABLEHDR*)(entry + 1);
        auto jumpTable = (int*)(jumpTableHdr + 1);
        log.Log("  iBaseClassIndex: %d\n", jumpTableHdr->iBaseClassIndex);
        log.Log("  iFirstDrawObjIndex: %d\n", jumpTableHdr->iFirstDrawObjIndex);
        log.Log("  iFirstTextObjIndex: %d\n", jumpTableHdr->iFirstTextObjIndex);
        log.Log("  cParts: %d\n", jumpTableHdr->cParts);
        for (int i = 0; i < jumpTableHdr->cParts; ++i)
            log.Log("  [%d] %d\n", i, jumpTable[i]);
    } else if (entry->usTypeNum == TMT_STATEJUMPTBL) {
        auto jumpTableHdr = (STATEJUMPTABLEHDR*)(entry + 1);
        auto jumpTable = (int*)(jumpTableHdr + 1);
        log.Log("  cStates: %d\n", jumpTableHdr->cStates);
        for (int i = 0; i < jumpTableHdr->cStates; ++i)
            log.Log("  [%d] %d\n", i, jumpTable[i]);
    } else if (entry->usTypeNum == TMT_IMAGEINFO) {
        auto data = (char*)(entry + 1);
        auto imageCount = *(char*)data;
        log.Log("  ImageCount: %d\n", imageCount);

        auto regionOffsets = (int*)(data + 8);

        for (unsigned i = 0; i < imageCount; ++i)
            log.Log("  [Region %d]: %d\n", i, regionOffsets[i]);

        for (unsigned i = 0; i < imageCount; ++i)
            DumpEntry(log, hdr, (ENTRYHDR*)((char*)hdr + regionOffsets[i]));
    } else if (entry->usTypeNum == TMT_REGIONDATA) {
        auto data = (RGNDATA*)(entry + 1);
        log.Log("  rdh.nCount:   %lu\n", data->rdh.nCount);
        log.Log("  rdh.nRgnSize: %lu\n", data->rdh.nRgnSize);
        log.Log("  rdh.rcBound: (%d,%d,%d,%d)\n", data->rdh.rcBound.left,
                data->rdh.rcBound.top, data->rdh.rcBound.right,
                data->rdh.rcBound.bottom);
        auto rects = (RECT*)data->Buffer;
        for (DWORD i = 0; i < data->rdh.nCount; ++i) {
            log.Log("  [%lu]: (%d,%d,%d,%d)\n", i, rects[i].left,
                    rects[i].top, rects[i].right, rects[i].bottom);
        }
    } else if (entry->usTypeNum == TMT_DRAWOBJ) {
        auto objHdr = (PARTOBJHDR*)(entry + 1);
        log.Log("[p:%d, s:%d]\n", objHdr->iPartId, objHdr->iStateId);
        DumpDrawObj(log, hdr, (CDrawBase*)(objHdr + 1));
    }

    log.Outdent();
}

static void DumpHeader(THEMEHDR const* hdr, LogFile& log)
{
    log.Log("THEMEHDR\n");
    log.Log("========================================\n");

    log.LogPair("szSignature", hdr->szSignature);
    log.LogPair("dwVersion", hdr->dwVersion);
    log.LogPair("ftModifTimeStamp", hdr->ftModifTimeStamp);
    log.LogPair("dwTotalLength", hdr->dwTotalLength);
    log.LogPair("iDllNameOffset", hdr->iDllNameOffset, (wchar_t const*)Advance(hdr, hdr->iDllNameOffset));
    log.LogPair("iColorParamOffset", hdr->iColorParamOffset, (wchar_t const*)Advance(hdr, hdr->iColorParamOffset));
    log.LogPair("iSizeParamOffset", hdr->iSizeParamOffset, (wchar_t const*)Advance(hdr, hdr->iSizeParamOffset));
    log.LogPair("dwLangID", hdr->dwLangID);
    log.LogPair("iLoadDPI", hdr->iLoadDPI);
    log.LogPair("dwLoadDPIs", hdr->dwLoadDPIs);
    log.LogPair("iLoadPPI", hdr->iLoadPPI);
    log.LogPair("iStringsOffset", hdr->iStringsOffset);
    log.LogPair("iStringsLength", hdr->iStringsLength);
    log.LogPair("iSectionIndexOffset", hdr->iSectionIndexOffset);
    log.LogPair("iSectionIndexLength", hdr->iSectionIndexLength);
    log.LogPair("iGlobalsOffset", hdr->iGlobalsOffset);
    log.LogPair("iGlobalsTextObjOffset", hdr->iGlobalsTextObjOffset);
    log.LogPair("iGlobalsDrawObjOffset", hdr->iGlobalsDrawObjOffset);
    log.LogPair("iSysMetricsOffset", hdr->iSysMetricsOffset);
    log.LogPair("iFontsOffset", hdr->iFontsOffset);
    log.LogPair("cFonts", hdr->cFonts);
    log.Log("\n");
}

static void DumpSectionIndex(THEMEHDR const* hdr, LogFile& log)
{
    log.Log("SectionIndex\n");
    log.Log("========================================\n");

    auto begin = (APPCLASSLIVE*)Advance(hdr, hdr->iSectionIndexOffset);
    auto end = Advance(begin, hdr->iSectionIndexLength);

    for (auto p = begin; p < end; ++p) {
        auto appName = p->AppClassInfo.iAppNameIndex ?
            (wchar_t const*)Advance(hdr, p->AppClassInfo.iAppNameIndex) : L"<no app>";
        auto className = p->AppClassInfo.iClassNameIndex ?
            (wchar_t const*)Advance(hdr, p->AppClassInfo.iClassNameIndex) : L"<no class>";

        log.Log("%-10ls %-30ls  idx:%05d len:%05d base:%05d\n",
                appName, className, p->iIndex, p->iLen, p->iBaseClassIndex);

        auto be = (ENTRYHDR*)Advance(hdr, p->iIndex);
        auto ee = Advance(be, p->iLen);

        for (auto pe = be; pe < ee; pe = pe->Next()) {
            DumpEntry(log, hdr, pe);
        }
    }

    log.Log("\n");
}

static void DumpFonts(THEMEHDR const* hdr, LogFile& log)
{
    log.Log("Fonts\n");
    log.Log("========================================\n");

    auto fonts = (LOGFONTW const*)Advance(hdr, hdr->iFontsOffset);

    for (int i = 0; i < hdr->cFonts; ++i) {
        auto const& font = fonts[i];
        log.Log("Font %d\n", i);
        log.Indent();
        log.LogPair("lfHeight", font.lfHeight);
        log.LogPair("lfWidth", font.lfWidth);
        log.LogPair("lfEscapement", font.lfEscapement);
        log.LogPair("lfOrientation", font.lfOrientation);
        log.LogPair("lfWeight", font.lfWeight);
        log.LogPair("lfItalic", font.lfItalic);
        log.LogPair("lfUnderline", font.lfUnderline);
        log.LogPair("lfStrikeOut", font.lfStrikeOut);
        log.LogPair("lfCharSet", font.lfCharSet);
        log.LogPair("lfOutPrecision", font.lfOutPrecision);
        log.LogPair("lfClipPrecision", font.lfClipPrecision);
        log.LogPair("lfQuality", font.lfQuality);
        log.LogPair("lfPitchAndFamily", font.lfPitchAndFamily);
        log.LogPair("lfFaceName", font.lfFaceName);
        log.Outdent();
    }

    log.Log("\n");
}

static HRESULT WriteFileAllBytes(wchar_t const* path, void const* ptr, unsigned length)
{
    FileHandle file{CreateFileW(path, GENERIC_WRITE, 0, nullptr,
                                CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr)};

    while (length > 0) {
        DWORD bytesWritten;
        if (!WriteFile(file, ptr, length, &bytesWritten, nullptr))
            return MakeErrorLast();

        length -= bytesWritten;
        ptr = Advance(ptr, bytesWritten);
    }

    return S_OK;
}

static HRESULT DumpThemeFile(CUxThemeFile* themeFile, wchar_t const* path,
                             bool packed, bool fullInfo)
{
    THEMEHDR const* themeHdr = themeFile->_pbSharableData;

    if (packed)
        return WriteFileAllBytes(path, themeHdr, themeHdr->dwTotalLength);

    LogFile log{path};
    if (!log.Open())
        return E_FAIL;

    std::vector<std::wstring> str1;
    GetStrings(themeHdr, str1);

    DumpHeader(themeHdr, log);
    DumpSectionIndex(themeHdr, log);
    DumpFonts(themeHdr, log);
    return S_OK;
}

HRESULT DumpLoadedThemeToTextFile(HTHEMEFILE hThemeFile, wchar_t const* path,
                                  bool packed, bool fullInfo)
{
    auto themeFile = ThemeFileFromHandle(hThemeFile);
    if (!themeFile)
        return E_HANDLE;
    return DumpThemeFile(themeFile, path, packed, fullInfo);
}

HRESULT DumpSystemThemeToTextFile(wchar_t const* path, bool packed, bool fullInfo)
{
    SectionHandle sharableSection;
    SectionHandle nonSharableSection;

    ENSURE_HR(CUxThemeFile::GetGlobalTheme(
        sharableSection.CloseAndGetAddressOf(),
        nonSharableSection.CloseAndGetAddressOf()));

    CUxThemeFile themeFile;
    ENSURE_HR(themeFile.OpenFromHandle(sharableSection, nonSharableSection, FILE_MAP_READ, true));
    sharableSection.Detach();
    nonSharableSection.Detach();

    return DumpThemeFile(&themeFile, path, packed, fullInfo);
}

} // namespace uxtheme

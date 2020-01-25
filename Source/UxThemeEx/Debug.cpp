#include "Debug.h"

#include "AnimationLoader.h"
#include "BorderFill.h"
#include "Global.h"
#include "Handle.h"
#include "ImageFile.h"
#include "RenderObj.h"
#include "ThemeUtils.h"
#include "Utils.h"
#include "UxThemeFile.h"
#include "UxThemeHelpers.h"

#include <array>
#include <unordered_set>

#include <strsafe.h>
#include <winnt.h>
#include <winternl.h>

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
static int Format(char* buffer, size_t bufferSize, T const& value)
{
    return sprintf_s(buffer, bufferSize, "%d", value);
}

static int Format(char* buffer, size_t bufferSize, void* value)
{
    return sprintf_s(buffer, bufferSize, "%p", value);
}

static int Format(char* buffer, size_t bufferSize, void const* value)
{
    return sprintf_s(buffer, bufferSize, "%p", value);
}

static int Format(char* buffer, size_t bufferSize, char value)
{
    return sprintf_s(buffer, bufferSize, "%d", (int)value);
}

static int Format(char* buffer, size_t bufferSize, int8_t value)
{
    return sprintf_s(buffer, bufferSize, "%d", (int)value);
}

static int Format(char* buffer, size_t bufferSize, uint8_t value)
{
    return sprintf_s(buffer, bufferSize, "%u", (unsigned)value);
}

static int Format(char* buffer, size_t bufferSize, int16_t value)
{
    return sprintf_s(buffer, bufferSize, "%d", value);
}

static int Format(char* buffer, size_t bufferSize, uint16_t value)
{
    return sprintf_s(buffer, bufferSize, "%u", value);
}

static int Format(char* buffer, size_t bufferSize, int32_t value)
{
    return sprintf_s(buffer, bufferSize, "%d", value);
}

static int Format(char* buffer, size_t bufferSize, uint32_t value)
{
    return sprintf_s(buffer, bufferSize, "%u", value);
}

static int Format(char* buffer, size_t bufferSize, long value)
{
    return sprintf_s(buffer, bufferSize, "%ld", value);
}

static int Format(char* buffer, size_t bufferSize, unsigned long value)
{
    return sprintf_s(buffer, bufferSize, "%lu", value);
}

static int Format(char* buffer, size_t bufferSize, int64_t value)
{
    return sprintf_s(buffer, bufferSize, "%lld", value);
}

static int Format(char* buffer, size_t bufferSize, uint64_t value)
{
    return sprintf_s(buffer, bufferSize, "%llu", value);
}

static int Format(char* buffer, size_t bufferSize, char const* value)
{
    return sprintf_s(buffer, bufferSize, "%s", value);
}

static int Format(char* buffer, size_t bufferSize, wchar_t const* value)
{
    return sprintf_s(buffer, bufferSize, "%ls", value);
}

static int Format(char* buffer, size_t bufferSize, float value)
{
    return sprintf_s(buffer, bufferSize, "%g", value);
}

static int Format(char* buffer, size_t bufferSize, double value)
{
    return sprintf_s(buffer, bufferSize, "%g", value);
}

static int Format(char* buffer, size_t bufferSize, POINT const& value)
{
    return sprintf_s(buffer, bufferSize, "(%ld,%ld)", value.x, value.y);
}

static int Format(char* buffer, size_t bufferSize, SIZE const& value)
{
    return sprintf_s(buffer, bufferSize, "(%ld,%ld)", value.cx, value.cy);
}

static int Format(char* buffer, size_t bufferSize, RECT const& value)
{
    return sprintf_s(buffer, bufferSize, "(%ld,%ld,%ld,%ld)", value.left, value.top,
                     value.right, value.bottom);
}

static int Format(char* buffer, size_t bufferSize, MARGINS const& value)
{
    return sprintf_s(buffer, bufferSize, "(l:%d,r:%d,t:%d,b:%d)", value.cxLeftWidth,
                     value.cxRightWidth, value.cyTopHeight, value.cyBottomHeight);
}

static int Format(char* buffer, size_t bufferSize, FILETIME const& value)
{
    return sprintf_s(buffer, bufferSize, "0x%llX",
                     ((DWORD64)value.dwLowDateTime << 32) | value.dwLowDateTime);
}

static int FormatHex(char* buffer, size_t bufferSize, unsigned value)
{
    return sprintf_s(buffer, bufferSize, "0x%08X", value);
}

static int FormatHex(char* buffer, size_t bufferSize, unsigned long value)
{
    return sprintf_s(buffer, bufferSize, "0x%08lX", value);
}

class LogFile
{
public:
    LogFile(wchar_t const* path)
        : path(path)
    {}

    bool Open()
    {
        FileHandle h{CreateFileW(path.c_str(), GENERIC_WRITE, 0, nullptr, CREATE_ALWAYS,
                                 FILE_ATTRIBUTE_NORMAL, nullptr)};
        if (!h)
            return false;
        hFile = std::move(h);
        return true;
    }

    void Indent(int n = 1) { indentLevel += n; }
    void Outdent(int n = 1) { indentLevel -= n; }

    void StartLine() { WriteIndent(); }

    template<typename... T>
    void LogNoIndent(char const* format, T const&... args)
    {
        int len = sprintf_s(buffer, countof(buffer), format, args...);
        if (len <= 0)
            return;

        DWORD written;
        WriteFile(hFile, buffer, static_cast<DWORD>(len), &written, nullptr);
    }

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

    template<typename T>
    void LogPairHex(char const* key, T const& value)
    {
        WriteIndent();

        DWORD written;

        int len = sprintf_s(buffer, countof(buffer), "%s: ", key);
        if (len > 0)
            WriteFile(hFile, buffer, static_cast<DWORD>(len), &written, nullptr);

        len = FormatHex(buffer, countof(buffer), value);
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
        LogPair("fPartiallyTransparent", value.fPartiallyTransparent != 0);
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

    template<typename T, size_t N>
    void LogPairHex(char const* key, T const (&value)[N])
    {
        DWORD written;

        for (size_t i = 0; i < N; ++i) {
            WriteIndent();

            int len = sprintf_s(buffer, countof(buffer), "%s[%zu]: ", key, i);
            if (len > 0)
                WriteFile(hFile, buffer, static_cast<DWORD>(len), &written, nullptr);

            len = FormatHex(buffer, countof(buffer), value[i]);
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
        WriteFile(hFile, indentBuffer.data(), static_cast<DWORD>(length), &written,
                  nullptr);
    }

    std::wstring path;
    FileHandle hFile;
    char buffer[0x800];
    std::array<char, 64> indentBuffer;
    int indentLevel = 0;
};

static void DumpFont(LogFile& log, LOGFONTW const& font)
{
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
}

static void DumpThemeMetrics(LogFile& log, THEMEMETRICS const& tm)
{
    for (size_t i = 0; i < countof(tm.lfFonts); ++i) {
        log.Log("Font %d\n", i);
        log.Indent();
        DumpFont(log, tm.lfFonts[i]);
        log.Outdent();
    }

    for (size_t i = 0; i < countof(tm.crColors); ++i)
        log.Log("Color %d: 0x%08X\n", i, tm.crColors[i]);

    for (size_t i = 0; i < countof(tm.iSizes); ++i)
        log.Log("Size %d: 0x%08X\n", i, tm.iSizes[i]);

    for (size_t i = 0; i < countof(tm.fBools); ++i)
        log.Log("Bool %d: %d\n", i, tm.fBools[i]);

    for (size_t i = 0; i < countof(tm.iStringOffsets); ++i)
        log.Log("StringOffset %d: %d\n", i, tm.iStringOffsets[i]);

    for (size_t i = 0; i < countof(tm.iInts); ++i)
        log.Log("Int %d: %d\n", i, tm.iInts[i]);
}

static void DumpDrawObj(LogFile& log, CDrawBase const* drawObj)
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
        log.LogPairHex("_crFill", imageFile->_crFill);
        log.LogPair("_iTrueSizeStretchMark", imageFile->_iTrueSizeStretchMark);
        log.LogPair("_fUniformSizing", imageFile->_fUniformSizing);
        log.LogPair("_fIntegralSizing", imageFile->_fIntegralSizing);
        log.LogPair("_SizingMargins", imageFile->_SizingMargins);
        log.LogPair("_ContentMargins", imageFile->_ContentMargins);
        log.LogPair("_fSourceGrow", imageFile->_fSourceGrow);
        log.LogPair("_fSourceShrink", imageFile->_fSourceShrink);
        log.LogPair("_fGlyphOnly", imageFile->_fGlyphOnly);
        log.LogPair("_eGlyphType", imageFile->_eGlyphType);
        log.LogPairHex("_crGlyphTextColor", imageFile->_crGlyphTextColor);
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
        log.LogPairHex("_crBorder", borderFill->_crBorder);
        log.LogPair("_iBorderSize", borderFill->_iBorderSize);
        log.LogPair("_iRoundCornerWidth", borderFill->_iRoundCornerWidth);
        log.LogPair("_iRoundCornerHeight", borderFill->_iRoundCornerHeight);
        log.LogPair("_eFillType", borderFill->_eFillType);
        log.LogPairHex("_crFill", borderFill->_crFill);
        log.LogPair("_iDibOffset", borderFill->_iDibOffset);
        log.LogPair("_ContentMargins", borderFill->_ContentMargins);
        log.LogPair("_iGradientPartCount", borderFill->_iGradientPartCount);
        log.LogPairHex("_crGradientColors", borderFill->_crGradientColors);
        log.LogPair("_iGradientRatios", borderFill->_iGradientRatios);
        log.LogPair("_iSourcePartId", borderFill->_iSourcePartId);
        log.LogPair("_iSourceStateId", borderFill->_iSourceStateId);
    } else {
        log.LogPair("_eBgType", drawObj->_eBgType);
        log.LogPair("_iUnused", drawObj->_iUnused);
    }
}

static void DumpDrawObj(LogFile& log, CTextDraw const* drawObj)
{
    log.LogPair("_fComposited", drawObj->_fComposited);
    log.LogPairHex("_crText", drawObj->_crText);
    log.LogPairHex("_crEdgeLight", drawObj->_crEdgeLight);
    log.LogPairHex("_crEdgeHighlight", drawObj->_crEdgeHighlight);
    log.LogPairHex("_crEdgeShadow", drawObj->_crEdgeShadow);
    log.LogPairHex("_crEdgeDkShadow", drawObj->_crEdgeDkShadow);
    log.LogPairHex("_crEdgeFill", drawObj->_crEdgeFill);
    log.LogPair("_ptShadowOffset", drawObj->_ptShadowOffset);
    log.LogPairHex("_crShadow", drawObj->_crShadow);
    log.LogPair("_eShadowType", drawObj->_eShadowType);
    log.LogPair("_iBorderSize", drawObj->_iBorderSize);
    log.LogPairHex("_crBorder", drawObj->_crBorder);
    log.LogPairHex("_crGlow", drawObj->_crGlow);
    log.LogPair("_fApplyOverlay", drawObj->_fApplyOverlay);
    log.LogPair("_iGlowSize", drawObj->_iGlowSize);
    log.LogPair("_iGlowIntensity", drawObj->_iGlowIntensity);
    log.LogPair("_iFontIndex", drawObj->_iFontIndex);
    log.LogPair("_fItalicFont", drawObj->_fItalicFont);
    log.LogPair("_iSourcePartId", drawObj->_iSourcePartId);
    log.LogPair("_iSourceStateId", drawObj->_iSourceStateId);
}

static void DumpAnimationTransform(LogFile& log, TA_TRANSFORM const* transform)
{
    switch (static_cast<int>(transform->eTransformType)) {
    case TATT_TRANSLATE_2D:
    case TATT_SCALE_2D:
    case TATT_ROTATE_2D:
    case TATT_SKEW_2D: {
        auto t = reinterpret_cast<TA_TRANSFORM_2D const*>(transform);
        log.LogPair("header.eTransformType", t->header.eTransformType);
        log.LogPair("header.dwTimingFunctionId", t->header.dwTimingFunctionId);
        log.LogPair("header.dwStartTime", t->header.dwStartTime);
        log.LogPair("header.dwDurationTime", t->header.dwDurationTime);
        log.LogPair("header.eFlags", t->header.eFlags);
        log.LogPair("rX", t->rX);
        log.LogPair("rY", t->rY);
        log.LogPair("rInitialX", t->rInitialX);
        log.LogPair("rInitialY", t->rInitialY);
        log.LogPair("rOriginX", t->rOriginX);
        log.LogPair("rOriginY", t->rOriginY);
        break;
    }
    case TATT_OPACITY: {
        auto t = reinterpret_cast<TA_TRANSFORM_OPACITY const*>(transform);
        log.LogPair("header.eTransformType", t->header.eTransformType);
        log.LogPair("header.dwTimingFunctionId", t->header.dwTimingFunctionId);
        log.LogPair("header.dwStartTime", t->header.dwStartTime);
        log.LogPair("header.dwDurationTime", t->header.dwDurationTime);
        log.LogPair("header.eFlags", t->header.eFlags);
        log.LogPair("rOpacity", t->rOpacity);
        log.LogPair("rInitialOpacity", t->rInitialOpacity);
        break;
    }
    case TATT_CLIP: {
        auto t = reinterpret_cast<TA_TRANSFORM_CLIP const*>(transform);
        log.LogPair("header.eTransformType", t->header.eTransformType);
        log.LogPair("header.dwTimingFunctionId", t->header.dwTimingFunctionId);
        log.LogPair("header.dwStartTime", t->header.dwStartTime);
        log.LogPair("header.dwDurationTime", t->header.dwDurationTime);
        log.LogPair("header.eFlags", t->header.eFlags);
        log.LogPair("rLeft", t->rLeft);
        log.LogPair("rTop", t->rTop);
        log.LogPair("rRight", t->rRight);
        log.LogPair("rBottom", t->rBottom);
        log.LogPair("rInitialLeft", t->rInitialLeft);
        log.LogPair("rInitialTop", t->rInitialTop);
        log.LogPair("rInitialRight", t->rInitialRight);
        log.LogPair("rInitialBottom", t->rInitialBottom);
        break;
    }
    case TATT_TRANSLATE_3D:
    case TATT_SCALE_3D:
    case TATT_ROTATE_3D:
    case TATT_SKEW_3D: {
        auto t = reinterpret_cast<TA_TRANSFORM_3D const*>(transform);
        log.LogPair("header.eTransformType", t->header.eTransformType);
        log.LogPair("header.dwTimingFunctionId", t->header.dwTimingFunctionId);
        log.LogPair("header.dwStartTime", t->header.dwStartTime);
        log.LogPair("header.dwDurationTime", t->header.dwDurationTime);
        log.LogPair("header.eFlags", t->header.eFlags);
        log.LogPair("rX", t->rX);
        log.LogPair("rY", t->rY);
        log.LogPair("rZ", t->rZ);
        log.LogPair("rInitialX", t->rInitialX);
        log.LogPair("rInitialY", t->rInitialY);
        log.LogPair("rInitialZ", t->rInitialZ);
        log.LogPair("rOriginX", t->rOriginX);
        log.LogPair("rOriginY", t->rOriginY);
        log.LogPair("rOriginZ", t->rOriginZ);
        break;
    }
    default:
        log.Log("<unknown transform type>\n");
        log.LogPair("eTransformType", transform->eTransformType);
        log.LogPair("dwTimingFunctionId", transform->dwTimingFunctionId);
        log.LogPair("dwStartTime", transform->dwStartTime);
        log.LogPair("dwDurationTime", transform->dwDurationTime);
        log.LogPair("eFlags", transform->eFlags);
        break;
    }
}

static void DumpBytes4(LogFile& log, void const* bytes, size_t size)
{
    size_t const itemsPerRow = 4;

    auto ptr = static_cast<unsigned const*>(bytes);
    auto end = ptr + (size / sizeof(unsigned));

    while (ptr < end) {
        log.StartLine();
        for (size_t i = 0; i < itemsPerRow && ptr < end; ++i) {
            if (i != 0)
                log.LogNoIndent(" ");
            log.LogNoIndent("%08X", *ptr++);
        }

        log.LogNoIndent("\n");
    }

    auto bytePtr = reinterpret_cast<uint8_t const*>(ptr);
    auto byteEnd = reinterpret_cast<uint8_t const*>(bytes) + size;
    log.StartLine();
    for (size_t i = 0; bytePtr < byteEnd; ++i) {
        if (i != 0)
            log.LogNoIndent(" ");
        log.LogNoIndent("%02X", *bytePtr++);
    }
    log.LogNoIndent("\n");
}

static void DumpBitmapHeader(LogFile& log, BITMAPINFOHEADER const& hdr)
{
    log.LogPair("biSize", hdr.biSize);
    log.LogPair("biWidth", hdr.biWidth);
    log.LogPair("biHeight", hdr.biHeight);
    log.LogPair("biPlanes", hdr.biPlanes);
    log.LogPair("biBitCount", hdr.biBitCount);
    log.LogPair("biCompression", hdr.biCompression);
    log.LogPair("biSizeImage", hdr.biSizeImage);
    log.LogPair("biXPelsPerMeter", hdr.biXPelsPerMeter);
    log.LogPair("biYPelsPerMeter", hdr.biYPelsPerMeter);
    log.LogPair("biClrUsed", hdr.biClrUsed);
    log.LogPair("biClrImportant", hdr.biClrImportant);
}

static HRESULT DumpBitmap(LogFile& log, TMBITMAPHEADER const& tmhdr, HBITMAP hbmp)
{
    log.Log("hbitmap: %s\n", hbmp ? "<handle>" : "0");
    if (!hbmp)
        return S_OK;

    if (hbmp) {
        DWORD* pixels;
        int width;
        int height;
        int bytesPerPixel;
        int bytesPerRow;

        BitmapPixels helper;
        ENSURE_HR(helper.OpenBitmap(nullptr, hbmp, false, &pixels, &width, &height,
                                    &bytesPerPixel, &bytesPerRow, nullptr, 0));

        log.Log("BITMAPINFOHEADER\n");
        log.Indent();
        DumpBitmapHeader(log, *helper.BitmapHeader());
        log.Outdent();

        log.Log("Pixels\n");
        log.Indent();
        for (int h = 0; h < height; ++h) {
            log.StartLine();
            for (int w = 0, we = bytesPerRow / 4; w < we; ++w) {
                if (w != 0)
                    log.LogNoIndent(" ");
                log.LogNoIndent("%08lX", *pixels++);
            }
            log.LogNoIndent("\n");
        }
        log.Outdent();
    } else {
        auto bmphdr = (BITMAPHEADER*)((BYTE*)&tmhdr + tmhdr.dwSize);
        if (!bmphdr)
            return E_FAIL;

        log.Log("BITMAPINFOHEADER\n");
        log.Indent();
        DumpBitmapHeader(log, bmphdr->bmih);
        log.Outdent();
    }

    return S_OK;
}

static void DumpBitmapHandle(LogFile& log, TMBITMAPHEADER const& tmhdr,
                             NONSHARABLEDATAHDR const* nsdHdr)
{
    auto bmpHandles = (HBITMAP64 const*)(Advance(nsdHdr, nsdHdr->iBitmapsOffset));

    log.Log("dwSize: %d\n", tmhdr.dwSize);
    log.Log("iBitmapIndex: %d\n", tmhdr.iBitmapIndex);
    log.Log("fPartiallyTransparent: %d\n", tmhdr.fPartiallyTransparent != 0);
    if (tmhdr.iBitmapIndex != -1 && tmhdr.iBitmapIndex < nsdHdr->cBitmaps) {
        HBITMAP hbmp = bmpHandles[tmhdr.iBitmapIndex].hBitmap;
        HRESULT hr = DumpBitmap(log, tmhdr, hbmp);
        if (FAILED(hr))
            log.Log("Failed to dump bitmap: hr=%08X\n", hr);
    }
}

static void DumpEntry(LogFile& log, ENTRYHDR* entry, CUxThemeFile const& themeFile)
{
    auto const hdr = themeFile.ThemeHeader();
    auto const nsdHdr = themeFile.NonSharableDataHeader();

    log.Indent();
    log.Log("Entry(%05u, %05u, index:%05d, len:%05u)\n", entry->usTypeNum,
            entry->ePrimVal, (int)((uintptr_t)entry - (uintptr_t)hdr), entry->dwDataLen);

    log.Indent();
    if (entry->usTypeNum == TMT_THEMEMETRICS) {
        DumpThemeMetrics(log, *(THEMEMETRICS*)(entry + 1));
    } else if (entry->usTypeNum >= TMT_DIBDATA && entry->usTypeNum <= TMT_DIBDATA5) {
        if (entry->ePrimVal == TMT_HBITMAP || entry->ePrimVal == TMT_BITMAPREF) {
            auto& tmhdr = *reinterpret_cast<TMBITMAPHEADER*>(entry + 1);
            DumpBitmapHandle(log, tmhdr, nsdHdr);
        } else {
            log.Log("<unhandled DIBDATA type>\n");
        }
    } else if (entry->usTypeNum == TMT_PARTJUMPTABLE) {
        auto jumpTableHdr = (PARTJUMPTABLEHDR*)(entry + 1);
        auto jumpTable = (int*)(jumpTableHdr + 1);
        log.Log("iBaseClassIndex: %d\n", jumpTableHdr->iBaseClassIndex);
        log.Log("iFirstDrawObjIndex: %d\n", jumpTableHdr->iFirstDrawObjIndex);
        log.Log("iFirstTextObjIndex: %d\n", jumpTableHdr->iFirstTextObjIndex);
        log.Log("cParts: %d\n", jumpTableHdr->cParts);
        for (int i = 0; i < jumpTableHdr->cParts; ++i)
            log.Log("[%d] %d\n", i, jumpTable[i]);
    } else if (entry->usTypeNum == TMT_STATEJUMPTABLE) {
        auto jumpTableHdr = (STATEJUMPTABLEHDR*)(entry + 1);
        auto jumpTable = (int*)(jumpTableHdr + 1);
        log.Log("cStates: %d\n", jumpTableHdr->cStates);
        for (int i = 0; i < jumpTableHdr->cStates; ++i)
            log.Log("[%d] %d\n", i, jumpTable[i]);
    } else if (entry->usTypeNum == TMT_JUMPTOPARENT) {
        auto data = (int64_t*)(entry + 1);
        log.LogPair("index", *data);
    } else if (entry->usTypeNum == TMT_DRAWOBJ) {
        auto objHdr = (PARTOBJHDR*)(entry + 1);
        log.Log("[p:%d, s:%d]\n", objHdr->iPartId, objHdr->iStateId);
        DumpDrawObj(log, (CDrawBase*)(objHdr + 1));
    } else if (entry->usTypeNum == TMT_TEXTOBJ) {
        auto objHdr = (PARTOBJHDR*)(entry + 1);
        log.Log("[p:%d, s:%d]\n", objHdr->iPartId, objHdr->iStateId);
        DumpDrawObj(log, (CTextDraw*)(objHdr + 1));
    } else if (entry->usTypeNum == TMT_RGNLIST) {
        auto data = (BYTE*)(entry + 1);
        auto imageCount = *(BYTE*)data;
        auto regionOffsets = (int*)(data + 8);

        log.Log("ImageCount: %d\n", imageCount);
        for (unsigned i = 0; i < imageCount; ++i) {
            log.Log("[Region %d]: %d\n", i, regionOffsets[i]);
            if (regionOffsets[i] != 0)
                DumpEntry(log, (ENTRYHDR*)((BYTE*)hdr + regionOffsets[i]), themeFile);
        }
    } else if (entry->usTypeNum == TMT_RGNDATA) {
        auto data = (RGNDATA*)(entry + 1);
        log.Log("rdh.nCount:   %lu\n", data->rdh.nCount);
        log.Log("rdh.nRgnSize: %lu\n", data->rdh.nRgnSize);
        log.Log("rdh.rcBound: (%d,%d,%d,%d)\n", data->rdh.rcBound.left,
                data->rdh.rcBound.top, data->rdh.rcBound.right, data->rdh.rcBound.bottom);
        auto rects = (RECT*)data->Buffer;
        for (DWORD i = 0; i < data->rdh.nCount; ++i) {
            log.Log("[%lu]: (%d,%d,%d,%d)\n", i, rects[i].left, rects[i].top,
                    rects[i].right, rects[i].bottom);
        }
    } else if (entry->usTypeNum == TMT_ENDOFCLASS) {
        // Empty
    } else if (entry->usTypeNum == TMT_ANIMATION) {
        auto header = (CTransformSerializer::Header*)(entry + 1);
        log.LogPair("cbSize", header->_cbSize);
        log.LogPair("dwOffsetProperty", header->_dwOffsetProperty);
        log.LogPair("dwOffsetTransform", header->_dwOffsetTransform);
        AnimationProperty const* prop = header->Property(entry->dwDataLen);
        log.Log("[AnimationProperty]\n");
        log.Indent();
        log.LogPair("eFlags", prop->eFlags);
        log.LogPair("dwTransformCount", prop->dwTransformCount);
        log.LogPair("dwStaggerDelay", prop->dwStaggerDelay);
        log.LogPair("dwStaggerDelayCap", prop->dwStaggerDelayCap);
        log.LogPair("rStaggerDelayFactor", prop->rStaggerDelayFactor);
        log.LogPair("dwZIndex", prop->dwZIndex);
        log.LogPair("dwBackgroundPartId", prop->dwBackgroundPartId);
        log.LogPair("dwTuningLevel", prop->dwTuningLevel);
        log.LogPair("rPerspective", prop->rPerspective);
        log.Outdent();

        auto transform = header->Transform();
        auto const end = (TA_TRANSFORM const*)Advance(entry, entry->dwDataLen);
        for (int i = 0; transform < end; ++i) {
            log.Log("[Transform %d]\n", i);
            log.Indent();
            DumpAnimationTransform(log, transform);
            log.Outdent();
            transform =
                Advance(transform, CTransformSerializer::GetTransformSize(transform));
        }
    } else if (entry->usTypeNum == TMT_TIMINGFUNCTION) {
        auto timingFunction = (TA_TIMINGFUNCTION const*)(entry + 1);
        log.LogPair("eTimingFunctionType", timingFunction->eTimingFunctionType);
    } else if (entry->ePrimVal == TMT_ENUM) {
        auto& value = *reinterpret_cast<int const*>(entry + 1);
        log.Log("ENUM: %d\n", value);
    } else if (entry->ePrimVal == TMT_INT) {
        auto& value = *reinterpret_cast<int const*>(entry + 1);
        log.Log("INT: %d\n", value);
    } else if (entry->ePrimVal == TMT_STRING) {
        auto& value = *reinterpret_cast<wchar_t const*>(entry + 1);
        log.Log("STRING: %ls\n", value);
    } else if (entry->ePrimVal == TMT_BOOL) {
        auto& value = *reinterpret_cast<BOOL const*>(entry + 1);
        log.Log("BOOL: %d\n", value);
    } else if (entry->ePrimVal == TMT_COLOR) {
        auto& color = *reinterpret_cast<COLORREF const*>(entry + 1);
        log.Log("COLOR: 0x%08X\n", color);
    } else if (entry->ePrimVal == TMT_MARGINS) {
        auto& margins = *reinterpret_cast<MARGINS const*>(entry + 1);
        log.Log("MARGINS\n");
        log.LogPair("cxLeftWidth", margins.cxLeftWidth);
        log.LogPair("cxRightWidth", margins.cxRightWidth);
        log.LogPair("cyTopHeight", margins.cyTopHeight);
        log.LogPair("cyBottomHeight", margins.cyBottomHeight);
    } else if (entry->ePrimVal == TMT_FILENAME) {
    } else if (entry->ePrimVal == TMT_SIZE) {
        auto& value = *reinterpret_cast<int const*>(entry + 1);
        log.Log("SIZE: %d\n", value);
    } else if (entry->ePrimVal == TMT_POSITION) {
        auto& size = *reinterpret_cast<POINT const*>(entry + 1);
        log.Log("POSITION\n");
        log.LogPair("x", size.x);
        log.LogPair("y", size.y);
    } else if (entry->ePrimVal == TMT_RECT) {
        auto& rect = *reinterpret_cast<RECT const*>(entry + 1);
        log.Log("RECT\n");
        log.LogPair("left", rect.left);
        log.LogPair("top", rect.top);
        log.LogPair("right", rect.right);
        log.LogPair("bottom", rect.bottom);
    } else if (entry->ePrimVal == TMT_FONT) {
        auto index = *reinterpret_cast<unsigned short const*>(entry + 1);
        log.Log("FONT: %u\n", index);
        DumpFont(log, *themeFile.GetFontByIndex(index));
    } else if (entry->ePrimVal == TMT_INTLIST) {
        auto& intList = *reinterpret_cast<INTLIST const*>(entry + 1);
        log.Log("INTLIST\n");
        log.LogPair("iValueCount", intList.iValueCount);
        for (int i = 0; i < intList.iValueCount; ++i)
            log.Log("[%d] %d\n", i, intList.iValues[i]);
    } else if (entry->ePrimVal == TMT_HBITMAP || entry->ePrimVal == TMT_BITMAPREF) {
        auto& tmhdr = *reinterpret_cast<TMBITMAPHEADER*>(entry + 1);
        DumpBitmapHandle(log, tmhdr, nsdHdr);
    } else if (entry->ePrimVal == TMT_DISKSTREAM) {
        DWORD* data = (DWORD*)(entry + 1);
        DWORD offset = data[0];
        DWORD size = data[1];
        log.Log("DISKSTREAM\n");
        log.LogPair("offset", offset);
        log.LogPair("size", size);
    } else if (entry->ePrimVal == TMT_STREAM) {
        log.Log("<unhandled entry type>\n");
    } else if (entry->ePrimVal == TMT_FLOAT) {
        float value;
        std::memcpy(&value, entry + 1, sizeof(float));
        log.Log("FLOAT: %g\n", value);
    } else if (entry->ePrimVal == TMT_FLOATLIST) {
        auto& floatList = *reinterpret_cast<INTLIST const*>(entry + 1);
        log.Log("FLOATLIST\n");
        log.LogPair("iValueCount", floatList.iValueCount);
        for (int i = 0; i < floatList.iValueCount; ++i) {
            float value;
            std::memcpy(&value, &floatList.iValues[i], sizeof(float));
            log.Log("[%d] %g\n", i, value);
        }
    } else if (entry->ePrimVal == TMT_HCCOLORTYPE) {
        auto& value = *reinterpret_cast<HIGHCONTRASTCOLOR const*>(entry + 1);
        log.Log("HIGHCONTRASTCOLOR: %d\n", value);
    } else {
        log.Log("<unhandled entry type>\n");
    }

    log.Outdent(2);
}

static void DumpHeader(LogFile& log, CUxThemeFile const& themeFile)
{
    THEMEHDR const* hdr = themeFile.ThemeHeader();
    NONSHARABLEDATAHDR const* nsdHdr = themeFile.NonSharableDataHeader();

    log.Log("THEMEHDR\n");
    log.Log("========================================\n");
    log.LogPair("szSignature", hdr->szSignature);
    log.LogPair("dwVersion", hdr->dwVersion);
    log.LogPair("ftModifTimeStamp", hdr->ftModifTimeStamp);
    log.LogPair("dwTotalLength", hdr->dwTotalLength);
    log.LogPair("iDllNameOffset", hdr->iDllNameOffset,
                (wchar_t const*)Advance(hdr, hdr->iDllNameOffset));
    log.LogPair("iColorParamOffset", hdr->iColorParamOffset,
                (wchar_t const*)Advance(hdr, hdr->iColorParamOffset));
    log.LogPair("iSizeParamOffset", hdr->iSizeParamOffset,
                (wchar_t const*)Advance(hdr, hdr->iSizeParamOffset));
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

    log.Log("NONSHARABLEDATAHDR\n");
    log.Log("========================================\n");
    log.LogPair("dwFlags", nsdHdr->dwFlags);
    log.LogPair("iLoadId", nsdHdr->iLoadId);
    log.LogPair("cBitmaps", nsdHdr->cBitmaps);
    log.LogPair("iBitmapsOffset", nsdHdr->iBitmapsOffset);
    log.Log("\n");
}

static void DumpSectionIndex(LogFile& log, CUxThemeFile const& themeFile)
{
    THEMEHDR const* hdr = themeFile.ThemeHeader();

    log.Log("SectionIndex\n");
    log.Log("========================================\n");

    auto begin = (APPCLASSLIVE*)Advance(hdr, hdr->iSectionIndexOffset);
    auto end = Advance(begin, hdr->iSectionIndexLength);

    for (auto p = begin; p < end; ++p) {
        auto appName = p->AppClassInfo.iAppNameIndex
                           ? (wchar_t const*)Advance(hdr, p->AppClassInfo.iAppNameIndex)
                           : L"<no app>";
        auto className =
            p->AppClassInfo.iClassNameIndex
                ? (wchar_t const*)Advance(hdr, p->AppClassInfo.iClassNameIndex)
                : L"<no class>";

        log.Log("%-10ls %-30ls  idx:%05d len:%05d base:%05d\n", appName, className,
                p->iIndex, p->iLen, p->iBaseClassIndex);

        auto be = (ENTRYHDR*)Advance(hdr, p->iIndex);
        auto ee = Advance(be, p->iLen);

        for (auto pe = be; pe < ee; pe = pe->Next())
            DumpEntry(log, pe, themeFile);
    }

    log.Log("\n");
}

static void DumpFonts(LogFile& log, CUxThemeFile const& themeFile)
{
    THEMEHDR const* hdr = themeFile.ThemeHeader();
    log.Log("Fonts\n");
    log.Log("========================================\n");

    auto fonts = (LOGFONTW const*)Advance(hdr, hdr->iFontsOffset);

    for (int i = 0; i < hdr->cFonts; ++i) {
        log.Log("Font %d\n", i);
        log.Indent();
        DumpFont(log, fonts[i]);
        log.Outdent();
    }

    log.Log("\n");
}

static HRESULT WriteFileAllBytes(wchar_t const* path, void const* ptr, unsigned length)
{
    FileHandle file{CreateFileW(path, GENERIC_WRITE, 0, nullptr, CREATE_ALWAYS,
                                FILE_ATTRIBUTE_NORMAL, nullptr)};

    while (length > 0) {
        DWORD bytesWritten;
        if (!WriteFile(file, ptr, length, &bytesWritten, nullptr))
            return MakeErrorLast();

        length -= bytesWritten;
        ptr = Advance(ptr, bytesWritten);
    }

    return S_OK;
}

static HRESULT DumpThemeFile(CUxThemeFile const& themeFile, wchar_t const* path,
                             bool packed, bool fullInfo)
{
    THEMEHDR const& themeHdr = *themeFile.ThemeHeader();
    if (packed)
        return WriteFileAllBytes(path, &themeHdr, themeHdr.dwTotalLength);

    LogFile log{path};
    if (!log.Open())
        return E_FAIL;

    DumpHeader(log, themeFile);
    DumpSectionIndex(log, themeFile);
    DumpFonts(log, themeFile);
    return S_OK;
}

HRESULT DumpLoadedThemeToTextFile(HTHEMEFILE hThemeFile, wchar_t const* path, bool packed,
                                  bool fullInfo)
{
    auto themeFile = ThemeFileFromHandle(hThemeFile);
    if (!themeFile)
        return E_HANDLE;
    return DumpThemeFile(*themeFile, path, packed, fullInfo);
}

HRESULT DumpSystemThemeToTextFile(wchar_t const* path, bool packed, bool fullInfo)
{
    SectionHandle sharableSection;
    SectionHandle nonSharableSection;

    ENSURE_HR(CUxThemeFile::GetGlobalTheme(sharableSection.CloseAndGetAddressOf(),
                                           nonSharableSection.CloseAndGetAddressOf()));

    CUxThemeFile themeFile;
    ENSURE_HR(themeFile.OpenFromHandle(sharableSection, nonSharableSection, FILE_MAP_READ,
                                       true));
    sharableSection.Detach();
    nonSharableSection.Detach();

    return DumpThemeFile(themeFile, path, packed, fullInfo);
}

} // namespace uxtheme

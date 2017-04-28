#pragma once
#include <cassert>
#include <vssym32.h>
#include <windows.h>

namespace uxtheme
{

enum TM_RESERVED_PROPVALS
{
    TMT_THEMEMETRICS = 1,
    //TMT_DIBDATA = 2,
    TMT_DIBDATA1 = 3,
    TMT_DIBDATA2 = 4,
    TMT_DIBDATA3 = 5,
    TMT_DIBDATA4 = 6,
    TMT_DIBDATA5 = 7,
    //TMT_GLYPHDIBDATA = 8,
    TMT_NTLDATA = 9,
    TMT_PARTJUMPTABLE = 10,
    TMT_STATEJUMPTABLE = 11,
    TMT_JUMPTOPARENT = 12,
    TMT_ENUMDEF = 13,
    TMT_ENUMVAL = 14,
    TMT_RESERVED1 = 15,
    TMT_DRAWOBJ = 16,
    TMT_TEXTOBJ = 17,
    TMT_RGNLIST = 18,
    TMT_RGNDATA = 19,
    TMT_ENDOFCLASS = 20,
    TMT_UNKNOWN = 21,
    TMT_DIBDATA6 = 22,
    TMT_DIBDATA7 = 23,
};

enum TMT_EXTRA
{
    TMT_IMAGEPROPERTIESTYPE = 240,
    TMT_HCCOLORTYPE = 241,
    TMT_BITMAPIMAGETYPE = 242,
    TMT_COMPOSEDIMAGETYPE = 243,
    TMT_SIMPLIFIEDIMAGE = 5100,
    TMT_HCSIMPLIFIEDIMAGE = 5101,
    TMT_HCGLYPHBGCOLOR = 5102,
    TMT_5106 = 5106,
    TMT_TRANSPARENTMARGINS = 5107,
    TMT_HCBORDERCOLOR = 5110,
    TMT_HCFILLCOLOR = 5111,
    TMT_HCTEXTCOLOR = 5112,
    TMT_HCEDGEHIGHLIGHTCOLOR = 5113,
    TMT_HCEDGESHADOWCOLOR = 5114,
    TMT_HCTEXTBORDERCOLOR = 5115,
    TMT_HCTEXTSHADOWCOLOR = 5116,
    TMT_HCGLOWCOLOR = 5117,
    TMT_HCHEADING1TEXTCOLOR = 5118,
    TMT_HCHEADING2TEXTCOLOR = 5119,
    TMT_HCBODYTEXTCOLOR = 5120,
    TMT_HCGLYPHCOLOR = 5121,
    TMT_HCHOTTRACKING = 5122,
    TMT_PPIPLATEAU1 = 5128,
    TMT_PPIPLATEAU2 = 5129,
    TMT_PPIPLATEAU3 = 5130,
    TMT_FIRSTPPIPLATEAU = TMT_PPIPLATEAU1,
    TMT_LASTPPIPLATEAU = TMT_PPIPLATEAU3,
    TMT_IMAGEPLATEAU1 = 5131,
    TMT_IMAGEPLATEAU2 = 5132,
    TMT_IMAGEPLATEAU3 = 5133,
    TMT_GLYPHIMAGEPLATEAU1 = 5134,
    TMT_GLYPHIMAGEPLATEAU2 = 5135,
    TMT_GLYPHIMAGEPLATEAU3 = 5136,
    TMT_CONTENTMARGINSPLATEAU1 = 5137,
    TMT_CONTENTMARGINSPLATEAU2 = 5138,
    TMT_CONTENTMARGINSPLATEAU3 = 5139,
    TMT_SIZINGMARGINSPLATEAU1 = 5140,
    TMT_SIZINGMARGINSPLATEAU2 = 5141,
    TMT_SIZINGMARGINSPLATEAU3 = 5142,
    TMT_FIRSTPLATEAURECORD = TMT_IMAGEPLATEAU1,
    TMT_LASTPLATEAURECORD = TMT_SIZINGMARGINSPLATEAU3,
    TMT_COMPOSEDIMAGEFILE = 5143,
    TMT_COMPOSEDIMAGEFILE1 = 5144,
    TMT_COMPOSEDIMAGEFILE2 = 5145,
    TMT_COMPOSEDIMAGEFILE3 = 5146,
    TMT_COMPOSEDIMAGEFILE4 = 5147,
    TMT_COMPOSEDIMAGEFILE5 = 5148,
    TMT_COMPOSEDGLYPHIMAGEFILE = 5149,
    TMT_COMPOSEDIMAGEFILE6 = 5152,
    TMT_COMPOSEDIMAGEFILE7 = 5153,
    TMT_ANIMATION = 20000,
    TMT_TIMINGFUNCTION = 20100,
};

enum HIGHCONTRASTCOLOR
{
    HCC_COLOR_ACTIVECAPTION = 0x0,
    HCC_COLOR_CAPTIONTEXT = 0x1,
    HCC_COLOR_BTNFACE = 0x2,
    HCC_COLOR_BTNTEXT = 0x3,
    HCC_COLOR_DESKTOP = 0x4,
    HCC_COLOR_GRAYTEXT = 0x5,
    HCC_COLOR_HOTLIGHT = 0x6,
    HCC_COLOR_INACTIVECAPTION = 0x7,
    HCC_COLOR_INACTIVECAPTIONTEXT = 0x8,
    HCC_COLOR_HIGHLIGHT = 0x9,
    HCC_COLOR_HIGHLIGHTTEXT = 0xA,
    HCC_COLOR_WINDOW = 0xB,
    HCC_COLOR_WINDOWTEXT = 0xC,
};

struct BITMAPHEADER
{
    BITMAPINFOHEADER bmih;
    unsigned masks[3];
};

inline int Map_COMPOSEDIMAGEFILE_To_DIBDATA(int id)
{
    switch (id) {
    case TMT_COMPOSEDIMAGEFILE1: return TMT_DIBDATA1;
    case TMT_COMPOSEDIMAGEFILE2: return TMT_DIBDATA2;
    case TMT_COMPOSEDIMAGEFILE3: return TMT_DIBDATA3;
    case TMT_COMPOSEDIMAGEFILE4: return TMT_DIBDATA4;
    case TMT_COMPOSEDIMAGEFILE5: return TMT_DIBDATA5;
    case TMT_COMPOSEDIMAGEFILE6: return TMT_DIBDATA6;
    case TMT_COMPOSEDIMAGEFILE7: return TMT_DIBDATA7;
    default:
        assert("FRE: FALSE");
        return 0;
    }
}

inline int Map_IMAGEFILE_To_DIBDATA(int id)
{
    switch (id) {
    case TMT_IMAGEFILE1: return 3;
    case TMT_IMAGEFILE2: return 4;
    case TMT_IMAGEFILE3: return 5;
    case TMT_IMAGEFILE4: return 6;
    case TMT_IMAGEFILE5: return 7;
    case TMT_IMAGEFILE6: return 22;
    case TMT_IMAGEFILE7: return 23;
    default:
        assert("FRE: FALSE");
        return 0;
    }
}

inline int Map_COMPOSEDIMAGEFILE_To_Ordinal(int id)
{
    switch (id) {
    case TMT_COMPOSEDIMAGEFILE1: return 0;
    case TMT_COMPOSEDIMAGEFILE2: return 1;
    case TMT_COMPOSEDIMAGEFILE3: return 2;
    case TMT_COMPOSEDIMAGEFILE4: return 3;
    case TMT_COMPOSEDIMAGEFILE5: return 4;
    case TMT_COMPOSEDIMAGEFILE6: return 5;
    case TMT_COMPOSEDIMAGEFILE7: return 6;
    default:
        assert("FRE: FALSE");
        return 6;
    }
}

inline int Map_IMAGEFILE_To_Ordinal(int id)
{
    switch (id) {
    case TMT_IMAGEFILE1: return 0;
    case TMT_IMAGEFILE2: return 1;
    case TMT_IMAGEFILE3: return 2;
    case TMT_IMAGEFILE4: return 3;
    case TMT_IMAGEFILE5: return 4;
    case TMT_IMAGEFILE6: return 5;
    case TMT_IMAGEFILE7: return 6;
    default:
        assert("FRE: FALSE");
        return 0;
    }
}

inline int Map_Ordinal_To_MINDPI(int i)
{
    switch (i) {
    case 0: return TMT_MINDPI1;
    case 1: return TMT_MINDPI2;
    case 2: return TMT_MINDPI3;
    case 3: return TMT_MINDPI4;
    case 4: return TMT_MINDPI5;
    case 5: return TMT_MINDPI6;
    case 6: return TMT_MINDPI7;
    default:
        assert("FRE: FALSE");
        return TMT_MINDPI1;
    }
}

inline int Map_MINDPI_To_Ordinal(int id)
{
    switch (id) {
    case TMT_MINDPI1: return 0;
    case TMT_MINDPI2: return 1;
    case TMT_MINDPI3: return 2;
    case TMT_MINDPI4: return 3;
    case TMT_MINDPI5: return 4;
    case TMT_MINDPI6: return 5;
    case TMT_MINDPI7: return 6;
    default:
        assert("FRE: FALSE");
        return 0;
    }
}

inline int Map_Ordinal_To_MINSIZE(int i)
{
    switch (i) {
    case 0: return TMT_MINSIZE1;
    case 1: return TMT_MINSIZE2;
    case 2: return TMT_MINSIZE3;
    case 3: return TMT_MINSIZE4;
    case 4: return TMT_MINSIZE5;
    case 5: return TMT_MINSIZE6;
    case 6: return TMT_MINSIZE7;
    default:
        assert("FRE: FALSE");
        return TMT_MINSIZE7;
    }
}

inline int Map_Ordinal_To_IMAGEFILE(int i)
{
    switch (i) {
    case 0: return TMT_IMAGEFILE1;
    case 1: return TMT_IMAGEFILE2;
    case 2: return TMT_IMAGEFILE3;
    case 3: return TMT_IMAGEFILE4;
    case 4: return TMT_IMAGEFILE5;
    case 5: return TMT_IMAGEFILE6;
    case 6: return TMT_IMAGEFILE7;
    default:
        assert("FRE: FALSE");
        return TMT_IMAGEFILE7;
    }
}

inline int Map_Ordinal_To_COMPOSEDIMAGEFILE(int i)
{
    switch (i) {
    case 0: return TMT_COMPOSEDIMAGEFILE1;
    case 1: return TMT_COMPOSEDIMAGEFILE2;
    case 2: return TMT_COMPOSEDIMAGEFILE3;
    case 3: return TMT_COMPOSEDIMAGEFILE4;
    case 4: return TMT_COMPOSEDIMAGEFILE5;
    case 5: return TMT_COMPOSEDIMAGEFILE6;
    case 6: return TMT_COMPOSEDIMAGEFILE7;
    default:
        assert("FRE: FALSE");
        return 5153;
    }
}

inline int GetDpiPlateauByIndex(int i)
{
    switch (i) {
    case 0: return 96;
    case 1: return 120;
    case 2: return 144;
    case 3: return 192;
    case 4: return 240;
    case 5: return 288;
    case 6: return 384;
    default: return -1;
    }
}

inline int Map_Ordinal_To_DIBDATA(int i)
{
    switch (i) {
    case 0: return TMT_DIBDATA1;
    case 1: return TMT_DIBDATA2;
    case 2: return TMT_DIBDATA3;
    case 3: return TMT_DIBDATA4;
    case 4: return TMT_DIBDATA5;
    case 5: return TMT_DIBDATA6;
    case 6: return TMT_DIBDATA7;
    default:
        assert("FRE: FALSE");
        return TMT_DIBDATA7;
    }
}

} // namespace uxtheme

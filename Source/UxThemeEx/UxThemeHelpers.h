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

#define TMT_5100 5100
#define TMT_5101 5101
#define TMT_5102 5102
#define TMT_5106 5106
#define TMT_TRANSPARENTMARGINS 5107
#define TMT_HCBORDERCOLOR 5110
#define TMT_HCFILLCOLOR 5111
#define TMT_HCTEXTCOLOR 5112
#define TMT_HCEDGEHIGHLIGHTCOLOR 5113
#define TMT_HCEDGESHADOWCOLOR 5114
#define TMT_HCTEXTBORDERCOLOR 5115
#define TMT_HCTEXTSHADOWCOLOR 5116
#define TMT_HCGLOWCOLOR 5117
#define TMT_HCHEADING1TEXTCOLOR 5118
#define TMT_HCHEADING2TEXTCOLOR 5119
#define TMT_HCBODYTEXTCOLOR 5120
#define TMT_5121 5121
#define TMT_HCHOTTRACKING 5122
#define TMT_5128 5128
#define TMT_5130 5130
#define TMT_5131 5131
#define TMT_5132 5132
#define TMT_5133 5133
#define TMT_5134 5134
#define TMT_5135 5135
#define TMT_5136 5136
#define TMT_5137 5137
#define TMT_5138 5138
#define TMT_5139 5139
#define TMT_5140 5140
#define TMT_5141 5141
#define TMT_5142 5142
#define TMT_COMPOSEDIMAGEFILE 5143
#define TMT_COMPOSEDIMAGEFILE1 5144
#define TMT_COMPOSEDIMAGEFILE2 5145
#define TMT_COMPOSEDIMAGEFILE3 5146
#define TMT_COMPOSEDIMAGEFILE4 5147
#define TMT_COMPOSEDIMAGEFILE5 5148
#define TMT_COMPOSEDGLYPHIMAGEFILE 5149
#define TMT_COMPOSEDIMAGEFILE6 5152
#define TMT_COMPOSEDIMAGEFILE7 5153
#define TMT_ANIMATION 20000
#define TMT_TIMINGFUNCTION 20100

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

inline int Map_Ordinal_To_DpiPlateau(int i)
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

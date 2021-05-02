using System;

namespace VS.FileFormats.MPD
{
    [Serializable]
    public class MPDTextureAnimation
    {
        public byte type;
        public byte[] datas;
        // type is datas[6]
        // type 01 is a sprite sheet animation, 20 bytes length
        // type 02 is a sprite translation, 32 bytes length
        // type 06 is 36 bytes length, manage mesh rotation (for doors, chest tops, levers, etc)

        // the same Texture animation type 1 can be called by several mesh group, so there must be a reference to this in groups or in an unknown section
        // sp, speed = higher value = slower sprite transition

        // TYPE 1 examples :
        //       iterations   num sprites        type sp  dec            X     Y     W     H
        // 01 03      03         04        01 00  01  01  00  00 00 00 a8 01 60 00 02 00 10 00
        // 01 04      07         08        01 00  01  01  00  00 00 00 80 01 a0 00 04 00 20 00
        // 01 01      03         04        01 00  01  01  00  00 00 00 a8 01 60 00 02 00 10 00
        // 01 02      07         08        01 00  01  01  00  00 00 00 80 01 a0 00 04 00 20 00
        // 00 03      07         08        01 00  01  01  00  00 00 00 80 01 a0 00 04 00 20 00
        // 00 02      03         04        01 00  01  01  00  00 00 00 a8 01 60 00 02 00 10 00

        // TYPE 2 examples :
        //                   type   sp   tex coords  dest coords           trans      maybe translation with the second UV axis
        // 3c c9 00 00 01 00  02    01     a0 40        20 20      a0 41   1f 0f   00 01 01 00 00 0e 1e 00 0e 00 0e 00 a0 00 40 00
        // 1c 11 00 00 01 00  02    01     a0 40        20 20      a1 4e   1f 0f   00 01 01 00 00 0e 1e 00 0e 00 0e 00 a1 00 51 00
        // fc 10 00 00 01 00  02    01     a0 40        20 20      a0 4f   1f 0f   00 01 01 00 00 0e 1e 00 0e 00 0e 00 a0 00 40 00
        // 14 0e 00 00 01 00  02    01     a0 40        20 20      a0 4b   1f 0f   00 01 00 00 00 0e 1e 00 0e 00 0e 00 a0 00 40 00
        // c4 0d 00 00 01 00  02    01     a0 40        20 20      a0 4b   1f 0f   00 01 00 00 00 0e 1e 00 0e 00 0e 00 a0 00 40 00
        // a4 0c 00 00 01 00  02    01     80 40        20 40      90 4d   0f 1f   00 01 00 00 00 00 0e 0e 1e 00 1e 00 90 00 40 00

        // TYPE 6 examples : 
        // target uint16
        // axis : y = 1, x = 2, z = 3
        // target             type  axis       rotations           bspeed   more rotations to make bounces, each byte is relative
        // 84 01  00 00 00 01  06    03  01 00 00 00  fe ff ff ff    03     00 00 00 00 0e 00 00 00 0e 00 00 00 00 00 00 00 00 00 00
        // 04 02  00 00 00 01  06    01  03 00 00 00  fe ff ff ff    02     00 00 00 00 0c 00 00 00 0c 00 00 00 00 00 00 00 00 00 00
        // 44 02  00 00 00 01  06    01  fe ff ff ff  02 00 00 00    02     00 00 00 00 04 00 00 00 04 00 00 00 00 00 00 00 00 00 00
    }
}

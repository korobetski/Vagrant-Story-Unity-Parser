using System;
namespace VS.FileFormats.EVT
{
    [Serializable]
    public class OPCode
    {
        public string name;
        public byte OP;
        public byte[] parameters;





        
        
        
        
        public static readonly byte[] OPLen = {
            0x01,
            0x0a,
            0x03,
            0x03,
            0x04,
            0x12,
            0x0b,
            0x0b,
            0x26,
            0x00,
            0x01,
            0x01,
            0x01,
            0x03,
            0x03,
            0x01,
            0x0b, // DialogShow(idDlg, Style, x,x, y, w, h, ?, ?, ?)
            0x04, // DialogText(idDlg, idText, ?)
            0x02, // DialogHide(idDlg)
            0x09, //
            0x02, //
            0x02, //
            0x04, // SplashScreenChoose
            0x06, // SplashScreenLoad
            0x07, // SplashScreenShow
            0x01, // SplashScreenHide
            0x01, // SplashScreenFadeIn
            0x04, //
            0x02, //
            0x02, //
            0x04, //
            0x05, //
            0x09, // ModelLoad(idChr, ?, idSHP, ?, ?, ?, ?, ?)
            0x06, //
            0x06, // ModelAnimate(idChr, ?, idAnim, ?, ?)
            0x05, // ModelSetAnimations
            0x07, //
            0x09, //
            0x0c, // ModelPosition(idChr, ?, px,px, py,py, pz,pz, rx, ?, ?)
            0x07, //
            0x0a, // ModelMoveTo(idChr, x,x,x,x, y,y,y,y)
            0x07, // ModelMoveTo2
            0x07, //
            0x06, //
            0x06, //
            0x07, //
            0x0a, //
            0x03, // ModeFree(idChr, ?)
            0x06, // ModelLoadAnimationsEx
            0x06, //
            0x06, //
            0x0b, // ModelRotate
            0x08, //
            0x07, //
            0x07, //
            0x05, //
            0x06, // ModelLookAt
            0x0a, //
            0x04, // ModelLoadAnimations
            0x01, // WaitForFile
            0x02, //
            0x02, //
            0x0a, // ModelIlluminate
            0x04, //
            0x06, //
            0x06, //
            0x03, // ModelControlViaScript
            0x01, //
            0x02, // SetEngineMode
            0x03, //
            0x03, //
            0x04, //
            0x01, //
            0x03, //
            0x04, //
            0x03, //
            0x03, //
            0x02, //
            0x01, //
            0x01, //
            0x04, // ModelControlViaBattleMode
            0x04, //
            0x05, //
            0x03, //
            0x04, // BattleOver
            0x02, //
            0x03, //
            0x04, //
            0x02, // SetHeadsUpDisplayMode(idMode)
            0x04, //
            0x07, //
            0x04, //
            0x07, //
            0x04, //
            0x03, //
            0x02, //
            0x03, //
            0x03, //
            0x02, //
            0x01, //
            0x02, //
            0x02, //
            0x03, //
            0x05, //
            0x0a, // LoadRoom(idZone, idRoom, ?, ?, ?, ?, ?, ?, ?)
            0x04, // LoadScene
            0x04, //
            0x02, //
            0x04, //
            0x02, // DisplayRoom
            0x01, //
            0x02, //
            0x08, // ModelColor
            0x04, //
            0x03, //
            0x02, //
            0x02, //
            0x01, //
            0x01, //
            0x05, //
            0x02, //
            0x03, //
            0x02, //
            0x05, //
            0x02, //
            0x03, //
            0x04, //
            0x01, //
            0x05, //SoundEffects0
            0x04, //
            0x05, //
            0x01, //
            0x03, //
            0x03, //SoundEffects5
            0x02, //SoundEffects6
            0x02, //
            0x02, //SoundEffects8
            0x01, //
            0x00, //
            0x00, //
            0x00, //
            0x00, //
            0x00, //
            0x05, //
            0x03, //MusicLoad
            0x02, //
            0x04, //MusicPlay
            0x03, //
            0x03, //
            0x03, //
            0x01, //
            0x02, //
            0x01, //
            0x02, //AudioUnknown1
            0x03, //
            0x05, //
            0x05, //
            0x02, //AudioSetPitch
            0x01, //AudioUnknown2
            0x02, //
            0x05, //
            0x02, // SplashScreenEffects
            0x02, // CameraZoomIn
            0x01, //
            0x01, //
            0x01, //
            0x02, //
            0x00, //
            0x02, //
            0x02, //
            0x05, //
            0x00, //
            0x00, //
            0x00, //
            0x00, //
            0x00, //
            0x00, //
            0x02, //
            0x08, //
            0x04, //
            0x07, //
            0x03, //
            0x07, //
            0x06, //
            0x01, //
            0x03, //
            0x02, //
            0x03, //
            0x01, //
            0x02, //
            0x01, //
            0x03, //
            0x07, // CameraDirection
            0x01, // CameraSetAngle
            0x03, // CameraLookAt
            0x03, //
            0x04, // ModelAnimateObject
            0x09, //
            0x00, //
            0x0a, //
            0x0b, //
            0x02, //
            0x0a, //
            0x03, //
            0x00, //
            0x00, //
            0x00, //
            0x00, //
            0x07, // CameraPosition
            0x01, // SetCameraPosition
            0x03, //
            0x03, //
            0x04, // CameraHeight
            0x09, //
            0x00, //
            0x0a, //
            0x0b, //
            0x02, //
            0x0a, //
            0x03, //
            0x00, //
            0x00, //
            0x02, //
            0x02, //
            0x02, // CameraWait(?)
            0x02, //
            0x06, //
            0x06, //
            0x05, //
            0x06, //
            0x05, //
            0x02, //
            0x03, //
            0x03, //
            0x04, // CameraZoom
            0x04, //
            0x04, // CameraZoomScalar
            0x05, //
            0x00, //
            0x06, //
            0x02, // Wait(numFrames)
            0x02, //
            0x05, //
            0x02, //
            0x02, //
            0x04, //
            0x02, //
            0x03, //
            0x01, //
            0x01, //
            0x01, //
            0x01, //
            0x01, //
            0x01, //
            0x01, //
            0x01, // return()
        };
    }
}

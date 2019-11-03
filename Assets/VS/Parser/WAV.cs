using System.Collections.Generic;
using System.IO;
using VS.Utils;

namespace VS.Parser
{

    public class WAV
    {
        private string _name = "";

        public WAV()
        {
            _name = "New Wave";
        }

        public void ExportWav()
        {
            /*
            [Bloc de déclaration d'un fichier au format WAVE]
            FileTypeBlocID  (4 octets) : Constante «RIFF»  (0x52,0x49,0x46,0x46)
            FileSize        (4 octets) : Taille du fichier moins 8 octets
            FileFormatID    (4 octets) : Format = «WAVE»  (0x57,0x41,0x56,0x45)
            */
            uint size = 8;
            List<byte> wavByte = new List<byte>();
            wavByte.AddRange(new byte[] { 0x52, 0x49, 0x46, 0x46 }); // "RIFF"
            wavByte.AddRange(new byte[] { (byte)((size & 0xFF000000) >> 24), (byte)((size & 0x00FF0000) >> 16), (byte)((size & 0x0000FF00) >> 8), (byte)(size & 0x000000FF) }); // size
            wavByte.AddRange(new byte[] { 0x57, 0x41, 0x56, 0x45 }); // "Wave"

            /*
            [Bloc décrivant le format audio]
            FormatBlocID    (4 octets) : Identifiant «fmt »  (0x66,0x6D, 0x74,0x20)
            BlocSize        (4 octets) : Nombre d'octets du bloc - 16  (0x10)
            AudioFormat     (2 octets) : Format du stockage dans le fichier (1: PCM, ...)
            NbrCanaux       (2 octets) : Nombre de canaux (de 1 à 6, cf. ci-dessous)
            Frequence       (4 octets) : Fréquence d'échantillonnage (en hertz) [Valeurs standardisées : 11 025, 22 050, 44 100 et éventuellement 48 000 et 96 000]
            BytePerSec      (4 octets) : Nombre d'octets à lire par seconde (c.-à-d., Frequence * BytePerBloc).
            BytePerBloc     (2 octets) : Nombre d'octets par bloc d'échantillonnage (c.-à-d., tous canaux confondus : NbrCanaux * BitsPerSample/8).
            BitsPerSample   (2 octets) : Nombre de bits utilisés pour le codage de chaque échantillon (8, 16, 24)
            */

            wavByte.AddRange(new byte[] { 0x66, 0x6D, 0x74, 0x20 }); // Identifiant «fmt »  (0x66,0x6D, 0x74,0x20)
            wavByte.AddRange(new byte[] { 0x10, 0x00, 0x00, 0x00 }); // Nombre d'octets du bloc - 16  (0x10)
            //https://en.wikipedia.org/wiki/Pulse-code_modulation
            wavByte.AddRange(new byte[] { 0x01, 0x00 }); // Format du stockage dans le fichier (1: PCM, ...)

            ToolBox.DirExNorCreate("Assets/Resources/Sounds/Wave/");
            using (FileStream fs = File.Create("Assets/Resources/Sounds/Wave/" + _name + ".wav"))
            {
                for (int i = 0; i < wavByte.Count; i++)
                {
                    fs.WriteByte(wavByte[i]);
                }
                fs.Close();
            }
        }
    }

}
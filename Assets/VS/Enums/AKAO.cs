namespace VS.Enums
{
    public class AKAO
    {
        public enum Type {
            UNKNOWN,
            SAMPLE_COLLECTION,
            SEQUENCE,
            PROG,
            SAMPLE,
            EFFECT
        }

        public enum Reverb
        {
            NO_REVERB,
            ROOM,
            STUDIO_SMALL,
            STUDIO_MEDIUM,
            STUDIO_LARGE,
            HALL,
            SPACE_ECHO,
            ECHO,
            DELAY,
            PIPE_ECHO,
        }

        public enum ADSRMode
        {
            DIRECT = 0,
            LINEAR_INCREASE = 1,
            LINEAR_INCREASE_INVERTED = 2,
            LINEAR_DECREASE = 3,
            LINEAR_DECREASE_INVERTED = 4,
            EXPONENTIAL_INCREASE = 5,
            EXPONENTIAL_INCREASE_INVERTED = 6,
            EXPONENTIAL_DECREASE = 7
        }
    }
}

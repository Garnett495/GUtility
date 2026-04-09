using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GUtility.Common.Recipe.Example
{
    /// <summary>
    /// 範例用的相機設定。
    /// </summary>
    [Serializable]
    public class GRecipeCameraConfig
    {
        /// <summary>
        /// 曝光值。
        /// </summary>
        public int Exposure { get; set; }

        /// <summary>
        /// 增益值。
        /// </summary>
        public double Gain { get; set; }

        /// <summary>
        /// 影像寬度。
        /// </summary>
        public int ImageWidth { get; set; }

        /// <summary>
        /// 影像高度。
        /// </summary>
        public int ImageHeight { get; set; }

        /// <summary>
        /// 建構子。
        /// </summary>
        public GRecipeCameraConfig()
        {
            Exposure = 0;
            Gain = 0.0;
            ImageWidth = 0;
            ImageHeight = 0;
        }
    }

    /// <summary>
    /// 範例用的馬達設定。
    /// </summary>
    [Serializable]
    public class GRecipeMotorConfig
    {
        /// <summary>
        /// 速度。
        /// </summary>
        public int Speed { get; set; }

        /// <summary>
        /// 加速度。
        /// </summary>
        public int Acceleration { get; set; }

        /// <summary>
        /// 減速度。
        /// </summary>
        public int Deceleration { get; set; }

        /// <summary>
        /// 建構子。
        /// </summary>
        public GRecipeMotorConfig()
        {
            Speed = 0;
            Acceleration = 0;
            Deceleration = 0;
        }
    }
}

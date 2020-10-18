using System.Diagnostics;
using Microsoft.Xna.Framework;
using log4net;
using CopperBend.Contract;
using CopperBend.Fabric.Tests;
using NSubstitute;
using NUnit.Framework;
using CopperBend.Creation;
using Size = System.Drawing.Size;
using Autofac;
using CopperBend.Model;

namespace CopperBend.Logic.Tests
{
    [TestFixture]
    public class Tests_Base
    {
        /// <summary> Mocked if GetServicesToMock returns correct flag </summary>
        protected ILog __log = null;
        /// <summary> Mocked if GetServicesToMock returns correct flag </summary>
        protected ISchedule __schedule = null;
        /// <summary> Mocked if GetServicesToMock returns correct flag </summary>
        protected IDescriber __describer = null;
        /// <summary> Mocked if GetServicesToMock returns correct flag </summary>
        protected ISadConEntityFactory __factory = null;
        /// <summary> Mocked if GetServicesToMock returns correct flag </summary>
        protected IMessager __messager = null;
        /// <summary> Mocked if GetServicesToMock returns correct flag </summary>
        protected IMessageLogWindow __msgLogWindow = null;
        /// <summary> Mocked if GetServicesToMock returns correct flag </summary>
        protected IControlPanel __controls;


        /// <summary>
        /// Override this as true in your test class, and the
        /// base class will configure the DI framework.
        /// </summary>
        protected virtual bool ShouldPrepDI => false;

        /// <summary>
        /// Override this to return the MockableServices flags
        /// describing the services you want to be mocked.
        /// Also will override the DI configuration with these
        /// mocks if ShouldPrepDI is overridden to be true.
        /// </summary>
        protected virtual MockableServices GetServicesToMock()
        {
            return MockableServices.None;
        }

        [SetUp]
        public void Tests_Base_SetUp()
        {
            Basis.ConnectIDGenerator();

            ContainerBuilder builder = null;
            if (ShouldPrepDI)
            {
                SourceMe.PreBuild(new Size(4, 4));
                builder = SourceMe.Builder;
            }

            var mocks = GetServicesToMock();

            if (mocks.HasFlag(MockableServices.Log))
            {
                __log = Substitute.For<ILog>();
                if (ShouldPrepDI) builder.RegisterInstance<ILog>(__log);
            }

            if (mocks.HasFlag(MockableServices.Schedule))
            {
                __schedule = Substitute.For<ISchedule>();
                if (ShouldPrepDI) builder.RegisterInstance<ISchedule>(__schedule);
            }

            if (mocks.HasFlag(MockableServices.Describer))
            {
                __describer = Substitute.For<IDescriber>();
                if (ShouldPrepDI) builder.RegisterInstance<IDescriber>(__describer);
            }

            if (mocks.HasFlag(MockableServices.Messager))
            {
                __messager = Substitute.For<IMessager>();
                if (ShouldPrepDI) builder.RegisterInstance<IMessager>(__messager);
            }

            if (mocks.HasFlag(MockableServices.EntityFactory))
            {
                __factory = UTHelp.GetSubstituteFactory();
                if (ShouldPrepDI) builder.RegisterInstance<ISadConEntityFactory>(__factory);
            }

            if (mocks.HasFlag(MockableServices.MessageLogWindow))
            {
                __msgLogWindow = Substitute.For<IMessageLogWindow>();
                if (ShouldPrepDI) builder.RegisterInstance<IMessageLogWindow>(__msgLogWindow);
            }

            if (mocks.HasFlag(MockableServices.ControlPanel))
            {
                __controls = Substitute.For<IControlPanel>();
                if (ShouldPrepDI) builder.RegisterInstance<IControlPanel>(__controls);
            }

            if (ShouldPrepDI) SourceMe.FinishBuild();
        }

        public void DBG() { if (!Debugger.IsAttached) Debugger.Launch(); }

        #region Color helpers

        private readonly Color[] DefinedColors =
        {
            Color.AliceBlue,
            Color.AntiqueWhite,
            Color.Aqua,
            Color.Aquamarine,
            Color.Azure,
            Color.Beige,
            Color.Bisque,
            Color.Black,
            Color.BlanchedAlmond,
            Color.Blue,
            Color.BlueViolet,
            Color.Brown,
            Color.BurlyWood,
            Color.CadetBlue,
            Color.Chartreuse,
            Color.Chocolate,
            Color.Coral,
            Color.CornflowerBlue,
            Color.Cornsilk,
            Color.Crimson,
            Color.Cyan,
            Color.DarkBlue,
            Color.DarkCyan,
            Color.DarkGoldenrod,
            Color.DarkGray,
            Color.DarkGreen,
            Color.DarkKhaki,
            Color.DarkMagenta,
            Color.DarkOliveGreen,
            Color.DarkOrange,
            Color.DarkOrchid,
            Color.DarkRed,
            Color.DarkSalmon,
            Color.DarkSeaGreen,
            Color.DarkSlateBlue,
            Color.DarkSlateGray,
            Color.DarkTurquoise,
            Color.DarkViolet,
            Color.DeepPink,
            Color.DeepSkyBlue,
            Color.DimGray,
            Color.DodgerBlue,
            Color.Firebrick,
            Color.FloralWhite,
            Color.ForestGreen,
            Color.Fuchsia,
            Color.Gainsboro,
            Color.GhostWhite,
            Color.Gold,
            Color.Goldenrod,
            Color.Gray,
            Color.Green,
            Color.GreenYellow,
            Color.Honeydew,
            Color.HotPink,
            Color.IndianRed,
            Color.Indigo,
            Color.Ivory,
            Color.Khaki,
            Color.Lavender,
            Color.LavenderBlush,
            Color.LawnGreen,
            Color.LemonChiffon,
            Color.LightBlue,
            Color.LightCoral,
            Color.LightCyan,
            Color.LightGoldenrodYellow,
            Color.LightGray,
            Color.LightGreen,
            Color.LightPink,
            Color.LightSalmon,
            Color.LightSeaGreen,
            Color.LightSkyBlue,
            Color.LightSlateGray,
            Color.LightSteelBlue,
            Color.LightYellow,
            Color.Lime,
            Color.LimeGreen,
            Color.Linen,
            Color.Magenta,
            Color.Maroon,
            Color.MediumAquamarine,
            Color.MediumBlue,
            Color.MediumOrchid,
            Color.MediumPurple,
            Color.MediumSeaGreen,
            Color.MediumSlateBlue,
            Color.MediumSpringGreen,
            Color.MediumTurquoise,
            Color.MediumVioletRed,
            Color.MidnightBlue,
            Color.MintCream,
            Color.MistyRose,
            Color.Moccasin,
            Color.MonoGameOrange,
            Color.NavajoWhite,
            Color.Navy,
            Color.OldLace,
            Color.Olive,
            Color.OliveDrab,
            Color.Orange,
            Color.OrangeRed,
            Color.Orchid,
            Color.PaleGoldenrod,
            Color.PaleGreen,
            Color.PaleTurquoise,
            Color.PaleVioletRed,
            Color.PapayaWhip,
            Color.PeachPuff,
            Color.Peru,
            Color.Pink,
            Color.Plum,
            Color.PowderBlue,
            Color.Purple,
            Color.Red,
            Color.RosyBrown,
            Color.RoyalBlue,
            Color.SaddleBrown,
            Color.Salmon,
            Color.SandyBrown,
            Color.SeaGreen,
            Color.SeaShell,
            Color.Sienna,
            Color.Silver,
            Color.SkyBlue,
            Color.SlateBlue,
            Color.SlateGray,
            Color.Snow,
            Color.SpringGreen,
            Color.SteelBlue,
            Color.Tan,
            Color.Teal,
            Color.Thistle,
            Color.Tomato,
            Color.Transparent,
            Color.Turquoise,
            Color.Violet,
            Color.Wheat,
            Color.White,
            Color.WhiteSmoke,
            Color.Yellow,
            Color.YellowGreen,
        };

        public enum ColorEn
        {
            AliceBlue,
            AntiqueWhite,
            Aqua,
            Aquamarine,
            Azure,
            Beige,
            Bisque,
            Black,
            BlanchedAlmond,
            Blue,
            BlueViolet,
            Brown,
            BurlyWood,
            CadetBlue,
            Chartreuse,
            Chocolate,
            Coral,
            CornflowerBlue,
            Cornsilk,
            Crimson,
            Cyan,
            DarkBlue,
            DarkCyan,
            DarkGoldenrod,
            DarkGray,
            DarkGreen,
            DarkKhaki,
            DarkMagenta,
            DarkOliveGreen,
            DarkOrange,
            DarkOrchid,
            DarkRed,
            DarkSalmon,
            DarkSeaGreen,
            DarkSlateBlue,
            DarkSlateGray,
            DarkTurquoise,
            DarkViolet,
            DeepPink,
            DeepSkyBlue,
            DimGray,
            DodgerBlue,
            Firebrick,
            FloralWhite,
            ForestGreen,
            Fuchsia,
            Gainsboro,
            GhostWhite,
            Gold,
            Goldenrod,
            Gray,
            Green,
            GreenYellow,
            Honeydew,
            HotPink,
            IndianRed,
            Indigo,
            Ivory,
            Khaki,
            Lavender,
            LavenderBlush,
            LawnGreen,
            LemonChiffon,
            LightBlue,
            LightCoral,
            LightCyan,
            LightGoldenrodYellow,
            LightGray,
            LightGreen,
            LightPink,
            LightSalmon,
            LightSeaGreen,
            LightSkyBlue,
            LightSlateGray,
            LightSteelBlue,
            LightYellow,
            Lime,
            LimeGreen,
            Linen,
            Magenta,
            Maroon,
            MediumAquamarine,
            MediumBlue,
            MediumOrchid,
            MediumPurple,
            MediumSeaGreen,
            MediumSlateBlue,
            MediumSpringGreen,
            MediumTurquoise,
            MediumVioletRed,
            MidnightBlue,
            MintCream,
            MistyRose,
            Moccasin,
            MonoGameOrange,
            NavajoWhite,
            Navy,
            OldLace,
            Olive,
            OliveDrab,
            Orange,
            OrangeRed,
            Orchid,
            PaleGoldenrod,
            PaleGreen,
            PaleTurquoise,
            PaleVioletRed,
            PapayaWhip,
            PeachPuff,
            Peru,
            Pink,
            Plum,
            PowderBlue,
            Purple,
            Red,
            RosyBrown,
            RoyalBlue,
            SaddleBrown,
            Salmon,
            SandyBrown,
            SeaGreen,
            SeaShell,
            Sienna,
            Silver,
            SkyBlue,
            SlateBlue,
            SlateGray,
            Snow,
            SpringGreen,
            SteelBlue,
            Tan,
            Teal,
            Thistle,
            Tomato,
            Transparent,
            Turquoise,
            Violet,
            Wheat,
            White,
            WhiteSmoke,
            Yellow,
            YellowGreen,
        }

        public Color ColorOf(ColorEn colorEn)
        {
            var index = (int)colorEn;
            return DefinedColors[index];
        }
        #endregion
    }
}

using MudBlazor;

namespace PriMap.Components
{
    public static class AppTheme
    {
        public static readonly MudTheme Default = new()
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#16897C",
                Secondary = "#0F2438",
                AppbarBackground = "#0F2438",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#0F2438",
                DrawerText = "rgba(255,255,255,0.85)",
                DrawerIcon = "rgba(255,255,255,0.7)",
                Background = "#F4F7F8",
                Surface = "#FFFFFF",
                TextPrimary = "#1E2A30",
                TextSecondary = "#55666E",
                Success = "#2E7D32",
                Warning = "#B26A00",
                Error = "#C62828",
                Info = "#0277BD",
                LinesDefault = "#E5EAEC"
            },
            PaletteDark = new PaletteDark
            {
                Primary = "#7FD9C4",
                Secondary = "#7FD9C4",
                AppbarBackground = "#0B1A28",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#0B1A28",
                DrawerText = "rgba(255,255,255,0.85)",
                DrawerIcon = "rgba(255,255,255,0.7)",
                Background = "#111B22",
                Surface = "#16232B",
                TextPrimary = "#E7EEF0",
                TextSecondary = "#A7B7BD",
                Success = "#66BB6A",
                Warning = "#FFB74D",
                Error = "#EF5350",
                Info = "#4FC3F7",
                LinesDefault = "#22323B"
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "8px"
            },
            Typography = new Typography
            {
                Default = new DefaultTypography
                {
                    FontFamily = ["Segoe UI", "Helvetica Neue", "Helvetica", "Arial", "sans-serif"]
                }
            }
        };
    }
}

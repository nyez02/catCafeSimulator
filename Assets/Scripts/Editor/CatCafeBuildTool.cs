#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class CatCafeBuildTool : Editor
{
    private static readonly string[] BuildScenes = new string[]
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/CafeScene.unity"
    };

    [MenuItem("Cat Cafe/📦 1-Click Xuất Bản Bản Web (WebGL - Chơi Trình Duyệt / Itch.io)", false, 10)]
    public static void BuildWebGLMenu()
    {
        string buildPath = Path.Combine(Directory.GetCurrentDirectory(), "Builds/CatCafe_Web");
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }

        Debug.Log("===> [BuildTool] Đang chuẩn bị đóng gói bản WebGL...");
        EnsureAppIcon();

        // Cấu hình chuẩn WebGL tương thích mọi hosting (Itch.io, GitHub Pages, Vercel)
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled; // Tránh lỗi CORS/Gzip header
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.runInBackground = true;

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = BuildScenes,
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildTool] Đóng gói WebGL thành công! Kích thước: {summary.totalSize / (1024 * 1024)} MB");
            EditorUtility.RevealInFinder(buildPath);
            EditorUtility.DisplayDialog("Xuất Bản WebGL Thành Công! 🎉",
                $"Đã đóng gói thành công bản Web tại:\n{buildPath}\n\n" +
                "👉 Cách chơi / chia sẻ:\n" +
                "1. Nén (Zip) toàn bộ nội dung trong thư mục này thành file .zip\n" +
                "2. Tải lên Itch.io (chọn loại project: HTML) để bạn bè mở link là chơi ngay trên trình duyệt!", "Tuyệt vời!");
        }
        else
        {
            Debug.LogError($"[BuildTool] Đóng gói WebGL thất bại: {summary.result}");
            EditorUtility.DisplayDialog("Lỗi Đóng Gói", "Có lỗi xảy ra khi đóng gói WebGL. Vui lòng kiểm tra Console/Editor Log.", "Đóng");
        }
    }

    [MenuItem("Cat Cafe/📦 1-Click Xuất Bản Bản Máy Tính (Windows PC .EXE)", false, 11)]
    public static void BuildWindowsMenu()
    {
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Builds/CatCafe_Windows");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string exePath = Path.Combine(folderPath, "CatCafeSimulator.exe");

        Debug.Log("===> [BuildTool] Đang chuẩn bị đóng gói bản Windows PC 64-bit...");
        EnsureAppIcon();

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = BuildScenes,
            locationPathName = exePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildTool] Đóng gói Windows PC thành công! File: {exePath}");
            EditorUtility.RevealInFinder(exePath);
            EditorUtility.DisplayDialog("Xuất Bản Windows PC Thành Công! 🎉",
                $"Đã đóng gói thành công file game .exe tại:\n{exePath}\n\n" +
                "👉 Bạn có thể nhấp đúp file CatCafeSimulator.exe để chơi ngay hoặc gửi cho bạn bè!", "Tuyệt vời!");
        }
        else
        {
            Debug.LogError($"[BuildTool] Đóng gói Windows thất bại: {summary.result}");
            EditorUtility.DisplayDialog("Lỗi Đóng Gói", "Có lỗi xảy ra khi đóng gói Windows. Vui lòng kiểm tra Console.", "Đóng");
        }
    }

    private static void EnsureAppIcon()
    {
        Texture2D iconTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Icon/app_icon.png");
        if (iconTex != null)
        {
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { iconTex });
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone, new Texture2D[] { iconTex });
            PlayerSettings.productName = "Cat Cafe Simulator";
            PlayerSettings.companyName = "Cozy Cat Studio";
        }
    }
}
#endif

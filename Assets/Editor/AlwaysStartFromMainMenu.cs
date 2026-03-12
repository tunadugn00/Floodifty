using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class AlwaysStartFromMainMenu
{
    // Key để lưu trạng thái bật/tắt
    const string PREF_KEY = "AlwaysStartMainMenu";

    // Đường dẫn trỏ thẳng đến MainMenu của ông
    const string SCENE_PATH = "Assets/Scenes/MainMenu.unity";

    // Hàm này tự động chạy khi Unity compile code
    static AlwaysStartFromMainMenu()
    {
        ApplyPlayModeStartScene();
    }

    // Đổi từ "Tuna Tools/..." thành "Tools/Tuna Tools/..."
    [MenuItem("Tools/Tuna Tools/🚀 Luôn Play từ Main Menu : BẬT")]
    static void Enable()
    {
        EditorPrefs.SetBool(PREF_KEY, true);
        ApplyPlayModeStartScene();
        Debug.Log("✅ Đã BẬT: Cứ ấn Play là về MainMenu nhé!");
    }

    [MenuItem("Tools/Tuna Tools/🚀 Luôn Play từ Main Menu : TẮT")]
    static void Disable()
    {
        EditorPrefs.SetBool(PREF_KEY, false);
        ApplyPlayModeStartScene();
        Debug.Log("❌ Đã TẮT: Ở Scene nào Play Scene đó.");
    }

    static void ApplyPlayModeStartScene()
    {
        // Mặc định là True (Bật)
        if (EditorPrefs.GetBool(PREF_KEY, true))
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(SCENE_PATH);
            if (sceneAsset != null)
            {
                EditorSceneManager.playModeStartScene = sceneAsset;
            }
            else
            {
                Debug.LogWarning("Ê, không tìm thấy MainMenu ở đường dẫn: " + SCENE_PATH + ". Nhớ check lại tên folder/file!");
            }
        }
        else
        {
            // Set null tức là vô hiệu hóa tính năng này
            EditorSceneManager.playModeStartScene = null;
        }
    }
}
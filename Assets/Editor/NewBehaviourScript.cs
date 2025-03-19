using UnityEngine;
using UnityEditor;
using UnityEditor.U2D;
using UnityEditor.U2D.Sprites; // SpriteDataProviderFactories 사용을 위해 추가
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 발생 이슈
    /*
        [Obsolete("Support for accessing sprite meta data through spritesheet has been removed. Please use the UnityEditor.U2D.Sprites.ISpriteEditorDataProvider interface instead.")]
        [NativeProperty("SpriteMetaDatas")]
        public extern SpriteMetaData[] spritesheet
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
            [MethodImpl(MethodImplOptions.InternalCall)]
            set;
        }
     */
/// 
/// 최신 코드 참고.
/// https://docs.unity3d.com/Packages/com.unity.2d.sprite@1.0/manual/DataProvider.html
/// 
/// </summary>

public class SpriteAtlasExporter : MonoBehaviour
{
    [MenuItem("Tools/Export Sprite Atlas to JSON")]
    static void ExportSpriteAtlas()
    {
        // 파일 선택 창 열기
        string texturePath = EditorUtility.OpenFilePanel("Select Sprite Atlas", Application.dataPath, "png");

        if (string.IsNullOrEmpty(texturePath))
        {
            Debug.Log("파일 선택이 취소되었습니다.");
            return;
        }

        // 프로젝트 상대 경로로 변환
        if (!texturePath.StartsWith(Application.dataPath))
        {
            Debug.LogError("선택한 파일이 프로젝트 Assets 폴더 안에 있어야 합니다.");
            return;
        }
        texturePath = "Assets" + texturePath.Substring(Application.dataPath.Length);

        // TextureImporter 가져오기
        TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (textureImporter == null || textureImporter.spriteImportMode != SpriteImportMode.Multiple)
        {
            Debug.LogError("스프라이트 아틀라스가 아니거나 올바른 경로가 아닙니다.");
            return;
        }

        // 스프라이트 데이터 가져오기
        List<SpriteData> spriteList = GetSpriteData(textureImporter);

        if (spriteList == null || spriteList.Count == 0)
        {
            Debug.LogError("스프라이트 데이터가 없습니다.");
            return;
        }

        // JSON 변환 및 저장
        SaveSpriteDataAsJson(texturePath, spriteList);
    }

    static List<SpriteData> GetSpriteData(TextureImporter textureImporter)
    {
        // `SpriteDataProviderFactories`를 사용하여 `ISpriteEditorDataProvider` 가져오기
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(textureImporter);

        if (provider == null)
        {
            Debug.LogError("ISpriteEditorDataProvider를 가져올 수 없습니다. Unity 2D Sprite 패키지가 설치되어 있는지 확인하세요.");
            return null;
        }

        // SpriteEditorDataProvider 초기화 후 데이터 가져오기
        provider.InitSpriteEditorDataProvider();
        var spriteRects = provider.GetSpriteRects(); // 스프라이트 메타데이터 가져오기
        provider.Apply(); // 변경 적용

        // Reimport하여 변경 적용
        var assetImporter = provider.targetObject as AssetImporter;
        assetImporter.SaveAndReimport();

        if (spriteRects == null || spriteRects.Length == 0)
        {
            Debug.LogError("스프라이트 데이터가 없습니다.");
            return null;
        }

        List<SpriteData> spriteList = new List<SpriteData>();
        foreach (var sprite in spriteRects)
        {
            spriteList.Add(new SpriteData
            {
                name = sprite.name,
                x = sprite.rect.x,
                y = sprite.rect.y,
                width = sprite.rect.width,
                height = sprite.rect.height,
                pivotX = sprite.pivot.x,
                pivotY = sprite.pivot.y
            });
        }

        return spriteList;
    }

    static void SaveSpriteDataAsJson(string texturePath, List<SpriteData> spriteList)
    {
        // 파일 이름 추출 (확장자 제외)
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(texturePath);

        // JSON 변환
        var spriteAtlas = new SpriteAtlas
        {
            fileName = fileNameWithoutExtension,
            prefix = "",
            sprites = spriteList
        };
        string json = JsonUtility.ToJson(spriteAtlas, true);

        // JSON 저장 경로 설정
        string savePath = EditorUtility.SaveFilePanel("Save JSON File", Application.dataPath, fileNameWithoutExtension, "json");
        if (string.IsNullOrEmpty(savePath))
        {
            Debug.Log("저장 취소됨.");
            return;
        }

        File.WriteAllText(savePath, json);
        Debug.Log($"JSON 저장 완료! {savePath}");
    }
}

// 스프라이트 데이터 구조체
[System.Serializable]
public class SpriteData
{
    public string name;
    public float x, y, width, height;
    public float pivotX, pivotY;
}

// JSON 변환을 위한 래퍼 클래스
[System.Serializable]
public class SpriteAtlas
{
    public string fileName;
    public string prefix;
    public List<SpriteData> sprites;
}

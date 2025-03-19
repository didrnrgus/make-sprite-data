using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

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

        // 파일 이름 추출 (확장자 제외)
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(texturePath);

        // TextureImporter 가져오기
        TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (textureImporter == null || textureImporter.spriteImportMode != SpriteImportMode.Multiple)
        {
            Debug.LogError("스프라이트 아틀라스가 아니거나 올바른 경로가 아닙니다.");
            return;
        }

        // 스프라이트 데이터 가져오기
        List<SpriteData> spriteList = new List<SpriteData>();
        foreach (var sprite in textureImporter.spritesheet)
        {
            SpriteData data = new SpriteData
            {

                name = sprite.name,
                x = sprite.rect.x,
                y = sprite.rect.y,
                width = sprite.rect.width,
                height = sprite.rect.height,
                pivotX = sprite.pivot.x,
                pivotY = sprite.pivot.y
            };
            spriteList.Add(data);
        }

        // JSON 변환
        var spriteAtlas = new SpriteAtlas 
        { 
            fileName = fileNameWithoutExtension,
            sprites = spriteList 
        };
        string json = JsonUtility.ToJson(spriteAtlas, true);

        // JSON 저장 경로 설정 (PNG 파일 이름을 기반으로 설정)
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
    public List<SpriteData> sprites;
}
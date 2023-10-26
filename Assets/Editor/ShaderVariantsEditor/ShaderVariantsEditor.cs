using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ShaderVariantsEditor : EditorWindow
{
    [MenuItem("Builds/ShaderVariantsEditor")]
    public static void ShowExample()
    {
        var window = GetWindow<ShaderVariantsEditor>("着色器变种收集工具", true);
        window.minSize = new Vector2(800, 600);
    }

    private Label _currentShaderCountField;
    private Label _currentVariantCountField;
    
    private TextField _collectOutputField;
    
    private SliderInt _processCapacitySlider;
    
    private Button _collectButton;
    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        var root = rootVisualElement;
        // Import UXML "Assets/Editor/ShaderVariantsEditor/ShaderVariantsEditor.uxml" 
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("016b73e2a897fa844a014bff395c6f0d"));
        visualTree.CloneTree(root);

        _currentShaderCountField = root.Q<Label>("CurrentShaderCount");
        _currentVariantCountField = root.Q<Label>("CurrentVariantCount");
        
        // 文件输出目录
        _collectOutputField = root.Q<TextField>("CollectOutput");
        _collectOutputField.SetValueWithoutNotify("Assets/GameRes/ShaderVariant/MyShaderVariants.shadervariants");
        _collectOutputField.RegisterValueChangedCallback(evt =>
        {
            // ShaderVariantCollectorSettingData.IsDirty = true;
            // ShaderVariantCollectorSettingData.Setting.SavePath = _collectOutputField.value;
        });
        
        // 容器值
        _processCapacitySlider = root.Q<SliderInt>("ProcessCapacity");
        _processCapacitySlider.SetValueWithoutNotify(1000);
        
        // 变种收集按钮
        _collectButton = root.Q<Button>("CollectButton");
        _collectButton.clicked += CollectButton_clicked;
    }
    
    private void CollectButton_clicked()
    {
        string savePath = ShaderVariantCollectorSettingData.Setting.SavePath;
        string packageName = ShaderVariantCollectorSettingData.Setting.CollectPackage;
        int processCapacity = _processCapacitySlider.value;
        ShaderVariantCollector.Run(savePath, packageName, processCapacity, null);
    }

    private void Update()
    {
        if (_currentShaderCountField != null)
        {
            int currentShaderCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionShaderCount();
            _currentShaderCountField.text = $"Current Shader Count : {currentShaderCount}";
        }

        if (_currentVariantCountField != null)
        {
            int currentVariantCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionVariantCount();
            _currentVariantCountField.text = $"Current Variant Count : {currentVariantCount}";
        }
    }
}

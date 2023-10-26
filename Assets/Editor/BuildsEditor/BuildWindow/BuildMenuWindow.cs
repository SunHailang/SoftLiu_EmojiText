using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


public class BuildMenuWindow : EditorWindow
{
    [MenuItem("Builds/Build Window")]
    static void OpenBuildWindow()
    {
        BuildMenuWindow window = EditorWindow.CreateWindow<BuildMenuWindow>("Build Window");
        window.Show();
        window.maxSize = new Vector2(850, 450);
        window.minSize = new Vector2(850, 450);
    }

    private void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        root.style.height = new StyleLength(new Length(100, LengthUnit.Percent));

        // UXML   BuildMenuWindow.uxml
        var uxmlDocument = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("ff44a0abcf61846429a39724d82d0ab5"));
        root.Add(uxmlDocument.Instantiate());

        // 获取组件
        Box pathBox = root.Q<Box>("PathBox");
        TextField outputPathText = pathBox.Q<TextField>("OutputPath");
        outputPathText.isReadOnly = true;
        string outputPath = BuildProcess.BuildInfoData.BuildOutputPath;
        outputPathText.value = outputPath;
        Button selectOutPath = pathBox.Q<Button>("SelecOutPath");
        selectOutPath.clicked += () =>
        {
            DirectoryInfo dirInfo = null;
            if (!Directory.Exists(outputPath))
            {
                dirInfo = Directory.CreateDirectory(outputPath);
            }
            else
            {
                dirInfo = new DirectoryInfo(outputPath);
            }

            outputPath = dirInfo.FullName;
            outputPath = EditorUtility.OpenFolderPanel("选择Build输出文件夹", outputPath, string.Empty);
            if (!string.IsNullOrEmpty(outputPath))
            {
                outputPathText.value = outputPath;
                BuildProcess.BuildInfoData.SetBuildOutputPath(outputPath);
            }
        };

        // RadioButton
        VisualElement buildVisualElement = root.Q<VisualElement>("BuildVisualElement");
        RadioButtonGroup buildPlatform = buildVisualElement.Q<RadioButtonGroup>("BuildPlatform");
        RadioButton windowRadio = buildPlatform.Q<RadioButton>("Windows");
        windowRadio.value = BuildProcess.BuildInfoData.BuildTarget == BuildTargetGroup.Standalone;
        windowRadio.RegisterValueChangedCallback((isOn) =>
        {
            if (isOn.newValue)
            {
                BuildProcess.BuildInfoData.SetBuildTarget(BuildTargetGroup.Standalone);
            }
        });
        RadioButton androidRadio = buildPlatform.Q<RadioButton>("Android");
        androidRadio.value = BuildProcess.BuildInfoData.BuildTarget == BuildTargetGroup.Android;
        Toggle exportAndroidProject = buildVisualElement.Q<Toggle>("ExportAndroidProject");
        exportAndroidProject.SetEnabled(androidRadio.value);
        androidRadio.RegisterValueChangedCallback((isOn) =>
        {
            exportAndroidProject.SetEnabled(isOn.newValue);
            BuildProcess.BuildInfoData.SetExportAndroidProject(isOn.newValue && exportAndroidProject.value);
            if (isOn.newValue)
            {
                BuildProcess.BuildInfoData.SetBuildTarget(BuildTargetGroup.Android);
            }
        });
        RadioButton iOSRadio = buildPlatform.Q<RadioButton>("iOS");
        iOSRadio.value = BuildProcess.BuildInfoData.BuildTarget == BuildTargetGroup.iOS;
        iOSRadio.RegisterValueChangedCallback((isOn) =>
        {
            if (isOn.newValue)
            {
                BuildProcess.BuildInfoData.SetBuildTarget(BuildTargetGroup.iOS);
            }
        });
        RadioButtonGroup buildType = buildVisualElement.Q<RadioButtonGroup>("BuildType");
        RadioButton devRadio = buildType.Q<RadioButton>("Dev");
        devRadio.value = BuildProcess.BuildInfoData.BuildType == BuildType.Development;
        devRadio.RegisterValueChangedCallback((isOn) =>
        {
            if (isOn.newValue)
            {
                BuildProcess.BuildInfoData.SetBuildType(BuildType.Development);
            }
        });
        RadioButton perRadio = buildType.Q<RadioButton>("Per");
        perRadio.value = BuildProcess.BuildInfoData.BuildType == BuildType.Preproduction;
        perRadio.RegisterValueChangedCallback((isOn) =>
        {
            if (isOn.newValue)
            {
                BuildProcess.BuildInfoData.SetBuildType(BuildType.Preproduction);
            }
        });
        RadioButton proRadio = buildType.Q<RadioButton>("Pro");
        proRadio.value = BuildProcess.BuildInfoData.BuildType == BuildType.Production;
        proRadio.RegisterValueChangedCallback((isOn) =>
        {
            if (isOn.newValue)
            {
                BuildProcess.BuildInfoData.SetBuildType(BuildType.Production);
            }
        });

        // Build Box
        Box buildBox = root.Q<Box>("BuildBox");
        Button buildOk = buildBox.Q<Button>("build");
        buildOk.clicked += Build_OnClick;
    }

    private void Build_OnClick()
    {
        BuildTarget target = BuildTarget.StandaloneWindows;
        switch (BuildProcess.BuildInfoData.BuildTarget)
        {
            case BuildTargetGroup.Android:
                target = BuildTarget.Android;
                break;
            case BuildTargetGroup.iOS:
                target = BuildTarget.iOS;
                break;
        }

        BuildProcess.Excute(target, BuildProcess.BuildInfoData.BuildType);
    }
}
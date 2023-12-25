using System.Collections.Generic;
using UnityEngine;
// Add this when creating in your own game namespace
// using LandscapeBuilder;

namespace LandscapeBuilder
{
	/// <summary>
	/// Example script that creates a landscape at runtime that shows how to import Group
	/// Object Path points from a set of JSON files.
	/// MANUAL STEPS REQUIRED
	/// 1. In the Application.dataPath folder, create a Heightmaps folder. In the Editor the Heightmaps
	///    folder should be created in the Project pane. For Standalone PC Builds, it should be added to
	///    the [project]_Data folder.
	/// 2. Create a duplicate of LandscapeBuilder/Modifiers/Hills/IsleOfEiggScotlandUK.raw
	/// 3. Copy the .raw file into the new Heightmaps folder.
	/// 4. Ensure the file in Heightmaps folder is the same name as the original.
	/// 5. Copy the RT8_CottageObjectPath1_Points* files from the LandscapeBuilder/Samples/Misc folder
	///    into a LB_JSON folder in the Application.dataPath folder (like you did for the Heightmaps).
	/// </summary>
	[RequireComponent(typeof(LBLandscape))]
	public class RuntimeSample08 : MonoBehaviour
	{
		#region Public Variables
		public Vector2 landscapeSize = Vector2.one * 1000f;
		public float terrainWidth = 1000f;
		public float terrainHeight = 300f;
		public Vector3 startPosition = new Vector3(-500f, 0f, -500f);

		public bool showErrors = false;
		public bool useGPUTopography = true;
		public bool useGPUTexturing = true;
		public bool useGPUGrass = true;
		public bool useGPUPath = true;

		[Header("Texture1")]
		public Texture2D textureTex1;
		public Texture2D normalMapTex1;
		public Texture2D heightMapTex1;
		public Texture2D mapTex1;

        [Header("Texture2")]
        public Texture2D textureTex2;
        public Texture2D normalMapTex2;
        public Texture2D heightMapTex2;
        public Texture2D mapTex2;
        #endregion

        #region Public Object Path Data
        [Header("ObjectPath Data")]
		[Tooltip("Default Asset or runtime data folder name for JSON files")]
		public string folderJSON = "LB_JSON";
		[Tooltip("Name of the JSON object path points file without the .json extension")]
		public string grp1Path1PointsFile = "";
		[Tooltip("Optionally resnap all the path points to the same height above the terrain")]
		public bool isSnapPathToTerrain = false;
        #endregion

        #region Private variables
        private LBLandscape landscape = null;
		private List<LBLayer> topographyLayers;
		private string objPathGUID = "1d21f62f-395e-4a4f-a0db-a84138df01bc";
		private LBObjPath lbObjPath = null;
		#endregion

		void Awake()
		{
			#region Initialise

			// This line just gets the starting time of the generation so that the total generation time
			// can be recorded and displayed
			float generationStartTime = Time.realtimeSinceStartup;

			RuntimeSampleHelper.RemoveDefaultCamera();
			//RuntimeSampleHelper.RemoveDefaultLight();

			// We're using some old trees in this demo, which don't like anti-aliasing
			QualitySettings.antiAliasing = 0;

			// Get a link to the LBLandscape script
			landscape = this.GetComponent<LBLandscape>();

			if (landscape == null)
			{
				Debug.Log("Could not add LBLandscape script to gameobject at Runtime");
				return;
			}

			else if (landscape.IsGPUAccelerationAvailable())
			{
				landscape.useGPUGrass = useGPUGrass;
				landscape.useGPUTexturing = useGPUTexturing;
				landscape.useGPUTopography = useGPUTopography;
				landscape.useGPUPath = useGPUPath;
			}
			else
			{
#if UNITY_EDITOR
				if (useGPUTopography || useGPUTexturing || useGPUGrass || useGPUPath)
				{
					Debug.Log("Sorry, your hardware does not support GPU acceleration");
				}
#endif
				landscape.useGPUTopography = false;
				landscape.useGPUTexturing = false;
				landscape.useGPUGrass = false;
				landscape.useGPUPath = false;
			}
			#endregion

			// Update the size
			landscape.size = landscapeSize;

			// Update the start position of landscape
			transform.position = startPosition;
			landscape.start = startPosition;

			#region Create the terrains
			int terrainNumber = 0;

			for (float tx = 0f; tx < landscapeSize.x - 1f; tx += terrainWidth)
			{
				for (float ty = 0f; ty < landscapeSize.y - 1f; ty += terrainWidth)
				{
					// Create a new gameobject
					GameObject terrainObj = new GameObject("RuntimeTerrain" + (terrainNumber++).ToString("000"));
					// Create a new gameobject
					// Correctly parent and position the terrain
					terrainObj.transform.parent = this.transform;
					terrainObj.transform.localPosition = new Vector3(tx, 0f, ty);
					// Add a terrain component
					Terrain newTerrain = terrainObj.AddComponent<Terrain>();
					// Set terrain settings)
					newTerrain.heightmapPixelError = 5f;
					newTerrain.basemapDistance = 5000f;
					newTerrain.treeDistance = 10000f;
					newTerrain.treeBillboardDistance = 200f;
					newTerrain.detailObjectDistance = 200f;
					newTerrain.treeCrossFadeLength = 5f;
					newTerrain.groupingID = 100;
					newTerrain.allowAutoConnect = true;
					// Set terrain data settings
					TerrainData newTerrainData = new TerrainData();

					newTerrainData.heightmapResolution = 2049;
					newTerrainData.size = new Vector3(terrainWidth, terrainHeight, terrainWidth);
					newTerrainData.SetDetailResolution(1024, 16);
					newTerrainData.wavingGrassSpeed = 0.5f;
					newTerrainData.wavingGrassAmount = 0.5f;
					newTerrainData.wavingGrassStrength = 0.5f;
					newTerrainData.wavingGrassTint = new Color(1f, 1f, 1f, 1f);
					newTerrain.terrainData = newTerrainData;

					// Set up the terrain collider
					TerrainCollider newTerrainCol = terrainObj.AddComponent<TerrainCollider>();
					newTerrainCol.terrainData = newTerrainData;

				}
			}
			#endregion

			landscape.SetLandscapeTerrains(true);

			landscape.SetDefaultTerrainMaterial();

			#region Topography Layer1
			AnimationCurve additiveCurveLyr1 = new AnimationCurve();

			AnimationCurve subtractiveCurveLyr1 = new AnimationCurve();

			List<LBLayerFilter> filtersLyr1 = new List<LBLayerFilter>();
			// Add LayerFilter code here
			List<AnimationCurve> imageCurveModifiersLyr1 = new List<AnimationCurve>();

			List<AnimationCurve> outputCurveModifiersLyr1 = new List<AnimationCurve>();

			List<LBCurve.CurvePreset> outputCurveModifierPresetsLyr1 = new List<LBCurve.CurvePreset>();

			List<AnimationCurve> perOctaveCurveModifiersLyr1 = new List<AnimationCurve>();
			AnimationCurve perOctaveCurveLyr11 = new AnimationCurve();
			perOctaveCurveLyr11.AddKey(0.00f, 0.50f);
			perOctaveCurveLyr11.AddKey(0.05f, 0.40f);
			perOctaveCurveLyr11.AddKey(0.20f, 0.10f);
			perOctaveCurveLyr11.AddKey(0.25f, 0.00f);
			perOctaveCurveLyr11.AddKey(0.30f, 0.20f);
			perOctaveCurveLyr11.AddKey(0.45f, 0.80f);
			perOctaveCurveLyr11.AddKey(0.50f, 1.00f);
			perOctaveCurveLyr11.AddKey(0.55f, 0.80f);
			perOctaveCurveLyr11.AddKey(0.70f, 0.20f);
			perOctaveCurveLyr11.AddKey(0.75f, 0.00f);
			perOctaveCurveLyr11.AddKey(0.80f, 0.10f);
			perOctaveCurveLyr11.AddKey(0.95f, 0.40f);
			perOctaveCurveLyr11.AddKey(1.00f, 0.50f);
			Keyframe[] perOctaveCurveLyr11Keys = perOctaveCurveLyr11.keys;
			perOctaveCurveLyr11Keys[0].inTangent = 0.00f;
			perOctaveCurveLyr11Keys[0].outTangent = 0.00f;
			perOctaveCurveLyr11Keys[1].inTangent = -2.00f;
			perOctaveCurveLyr11Keys[1].outTangent = -2.00f;
			perOctaveCurveLyr11Keys[2].inTangent = -2.00f;
			perOctaveCurveLyr11Keys[2].outTangent = -2.00f;
			perOctaveCurveLyr11Keys[3].inTangent = 0.00f;
			perOctaveCurveLyr11Keys[3].outTangent = 0.00f;
			perOctaveCurveLyr11Keys[4].inTangent = 4.00f;
			perOctaveCurveLyr11Keys[4].outTangent = 4.00f;
			perOctaveCurveLyr11Keys[5].inTangent = 4.00f;
			perOctaveCurveLyr11Keys[5].outTangent = 4.00f;
			perOctaveCurveLyr11Keys[6].inTangent = 0.00f;
			perOctaveCurveLyr11Keys[6].outTangent = 0.00f;
			perOctaveCurveLyr11Keys[7].inTangent = -4.00f;
			perOctaveCurveLyr11Keys[7].outTangent = -4.00f;
			perOctaveCurveLyr11Keys[8].inTangent = -4.00f;
			perOctaveCurveLyr11Keys[8].outTangent = -4.00f;
			perOctaveCurveLyr11Keys[9].inTangent = 0.00f;
			perOctaveCurveLyr11Keys[9].outTangent = 0.00f;
			perOctaveCurveLyr11Keys[10].inTangent = 2.00f;
			perOctaveCurveLyr11Keys[10].outTangent = 2.00f;
			perOctaveCurveLyr11Keys[11].inTangent = 2.00f;
			perOctaveCurveLyr11Keys[11].outTangent = 2.00f;
			perOctaveCurveLyr11Keys[12].inTangent = 0.00f;
			perOctaveCurveLyr11Keys[12].outTangent = 0.00f;
			perOctaveCurveLyr11 = new AnimationCurve(perOctaveCurveLyr11Keys);

			perOctaveCurveModifiersLyr1.Add(perOctaveCurveLyr11);

			List<LBCurve.CurvePreset> perOctaveCurveModifierPresetsLyr1 = new List<LBCurve.CurvePreset>();

			AnimationCurve mapPathBlendCurveLyr1 = new AnimationCurve();
			mapPathBlendCurveLyr1.AddKey(0.00f, 0.00f);
			mapPathBlendCurveLyr1.AddKey(1.00f, 1.00f);
			Keyframe[] mapPathBlendCurveLyr1Keys = mapPathBlendCurveLyr1.keys;
			mapPathBlendCurveLyr1Keys[0].inTangent = 0.00f;
			mapPathBlendCurveLyr1Keys[0].outTangent = 0.00f;
			mapPathBlendCurveLyr1Keys[1].inTangent = 0.00f;
			mapPathBlendCurveLyr1Keys[1].outTangent = 0.00f;
			mapPathBlendCurveLyr1 = new AnimationCurve(mapPathBlendCurveLyr1Keys);

			AnimationCurve mapPathHeightCurveLyr1 = new AnimationCurve();
			mapPathHeightCurveLyr1.AddKey(0.00f, 1.00f);
			mapPathHeightCurveLyr1.AddKey(1.00f, 1.00f);
			Keyframe[] mapPathHeightCurveLyr1Keys = mapPathHeightCurveLyr1.keys;
			mapPathHeightCurveLyr1Keys[0].inTangent = 0.00f;
			mapPathHeightCurveLyr1Keys[0].outTangent = 0.00f;
			mapPathHeightCurveLyr1Keys[1].inTangent = 0.00f;
			mapPathHeightCurveLyr1Keys[1].outTangent = 0.00f;
			mapPathHeightCurveLyr1 = new AnimationCurve(mapPathHeightCurveLyr1Keys);

			LBLayer lbBaseLayer1 = new LBLayer();
			if (lbBaseLayer1 != null)
			{
				lbBaseLayer1.layerName = "";
				lbBaseLayer1.type = LBLayer.LayerType.ImageModifier;
				lbBaseLayer1.preset = LBLayer.LayerPreset.MountainRangeBase;
				lbBaseLayer1.layerTypeMode = LBLayer.LayerTypeMode.Add;
				lbBaseLayer1.noiseTileSize = 5000;
				lbBaseLayer1.noiseOffsetX = 0;
				lbBaseLayer1.noiseOffsetZ = 0;
				lbBaseLayer1.octaves = 3;
				lbBaseLayer1.downscaling = 1;
				lbBaseLayer1.lacunarity = 1.95f;
				lbBaseLayer1.gain = 0.4f;
				lbBaseLayer1.additiveAmount = 0.5f;
				lbBaseLayer1.subtractiveAmount = 0.2f;
				lbBaseLayer1.additiveCurve = additiveCurveLyr1;
				lbBaseLayer1.subtractiveCurve = subtractiveCurveLyr1;
				lbBaseLayer1.removeBaseNoise = true;
				lbBaseLayer1.addMinHeight = false;
				lbBaseLayer1.addHeight = 0f;
				lbBaseLayer1.restrictArea = false;
				lbBaseLayer1.areaRect = new Rect(500, 500, 1000, 1000);
				lbBaseLayer1.interpolationSmoothing = 0;
				// lbBaseLayer1.heightmapImage = heightmapImageLyr1;
				lbBaseLayer1.imageHeightScale = 0.46f;
				lbBaseLayer1.imageCurveModifiers = imageCurveModifiersLyr1;
				lbBaseLayer1.filters = filtersLyr1;
				lbBaseLayer1.isDisabled = false;
				lbBaseLayer1.showLayer = true;
				lbBaseLayer1.showAdvancedSettings = false;
				lbBaseLayer1.showCurvesAndFilters = false;
				lbBaseLayer1.showAreaHighlighter = false;
				lbBaseLayer1.detailSmoothRate = 0f;
				lbBaseLayer1.areaBlendRate = 0.5f;
				lbBaseLayer1.downscaling = 1;
				lbBaseLayer1.warpAmount = 0f;
				lbBaseLayer1.warpOctaves = 1;
				lbBaseLayer1.outputCurveModifiers = outputCurveModifiersLyr1;
				lbBaseLayer1.outputCurveModifierPresets = outputCurveModifierPresetsLyr1;
				lbBaseLayer1.perOctaveCurveModifiers = perOctaveCurveModifiersLyr1;
				lbBaseLayer1.perOctaveCurveModifierPresets = perOctaveCurveModifierPresetsLyr1;
				lbBaseLayer1.heightScale = 0.75f;
				lbBaseLayer1.minHeight = 0f;
				lbBaseLayer1.imageSource = LBLayer.LayerImageSource.Default;
				lbBaseLayer1.imageRepairHoles = false;
				lbBaseLayer1.threshholdRepairHoles = 0f;
				lbBaseLayer1.pixelRangeRepairHoles = 6;
				lbBaseLayer1.normaliseImage = true;
				lbBaseLayer1.isBelowSeaLevelDataIncluded = false;
				lbBaseLayer1.floorOffsetY = 0f;
				lbBaseLayer1.mapPathBlendCurve = mapPathBlendCurveLyr1;
				lbBaseLayer1.mapPathHeightCurve = mapPathHeightCurveLyr1;
				lbBaseLayer1.mapPathAddInvert = false;

				// MANAUL STEPS REQUIRED
				// 1. In the Application.dataPath folder create a Heightmaps folder.
				// 2. If using a LB modifier, create a duplicate of the .raw file.
				// 3. Copy the .raw file into the new Heightmaps folder.
				// 4. Ensure the file in Heightmaps folder is the same name as the original.
				LBRaw lbRawLyr1 = LBRaw.ImportHeightmapRAW(Application.dataPath + "/Heightmaps/IsleOfEiggScotlandUK.RAW", false, false);
				if (lbRawLyr1 != null)
				{
					lbBaseLayer1.modifierRAWFile = lbRawLyr1;
				}
				lbBaseLayer1.modifierMode = LBLayer.LayerModifierMode.Set;
				lbBaseLayer1.modifierAddInvert = false;
				lbBaseLayer1.modifierUseBlending = false;
				lbBaseLayer1.modifierBlendingCentreSize = 0.71f;
				lbBaseLayer1.modifierBlendingFillCorners = 0f;
				lbBaseLayer1.areaRectRotation = 0f;
				lbBaseLayer1.modifierLandformCategory = LBModifierOperations.ModifierLandformCategory.Hills;
				lbBaseLayer1.modifierSourceFileType = LBRaw.SourceFileType.RAW;
				// Currently runtime does not support water with Modifier Layers - contact support or post in our Unity forum if you need this feature 
				lbBaseLayer1.modifierUseWater = false;
				lbBaseLayer1.modifierWaterIsMeshLandscapeUV = false;
				lbBaseLayer1.modifierWaterMeshUVTileScale = new Vector2(1f, 1f);
				// NOTE Add the new layer to the landscape meta-data
				landscape.topographyLayersList.Add(lbBaseLayer1);
			}
			#endregion

			// Create the terrain topographies
			landscape.ApplyTopography(false, showErrors);

			// Paste code generated from Trees Tab items here


			// Add Trees to the terrains
			#region Add Trees to Terrains
			landscape.treesHaveColliders = true;
			landscape.treePlacementSpeed = LBTerrainTree.TreePlacementSpeed.FastPlacement;
			landscape.ApplyTrees(true, showErrors);
			#endregion

			// Paste code generated from Texturing Tab items here
			#region LBTerrainTexture1
			AnimationCurve mapToleranceBlendCurveTex1 = new AnimationCurve();
			mapToleranceBlendCurveTex1.AddKey(0.00f, 0.00f);
			mapToleranceBlendCurveTex1.AddKey(0.50f, 0.13f);
			mapToleranceBlendCurveTex1.AddKey(1.00f, 1.00f);
			Keyframe[] mapToleranceBlendCurveTex1Keys = mapToleranceBlendCurveTex1.keys;
			mapToleranceBlendCurveTex1Keys[0].inTangent = 0.00f;
			mapToleranceBlendCurveTex1Keys[0].outTangent = 0.00f;
			mapToleranceBlendCurveTex1Keys[1].inTangent = 0.75f;
			mapToleranceBlendCurveTex1Keys[1].outTangent = 0.75f;
			mapToleranceBlendCurveTex1Keys[2].inTangent = 3.00f;
			mapToleranceBlendCurveTex1Keys[2].outTangent = 3.00f;
			mapToleranceBlendCurveTex1 = new AnimationCurve(mapToleranceBlendCurveTex1Keys);

			LBTerrainTexture lbTerrainTexture1 = new LBTerrainTexture();
			if (lbTerrainTexture1 != null)
			{
				lbTerrainTexture1.texture = textureTex1;
				lbTerrainTexture1.normalMap = normalMapTex1;
				lbTerrainTexture1.heightMap = heightMapTex1;
				lbTerrainTexture1.textureName = "Grass&Rock";
				lbTerrainTexture1.normalMapName = "";
				lbTerrainTexture1.tileSize = new Vector2(25, 25);
				lbTerrainTexture1.smoothness = 0f;
				lbTerrainTexture1.metallic = 0f;
				lbTerrainTexture1.minHeight = 0.25f;
				lbTerrainTexture1.maxHeight = 0.75f;
				lbTerrainTexture1.minInclination = 0f;
				lbTerrainTexture1.maxInclination = 30f;
				lbTerrainTexture1.strength = 0.01f;
				lbTerrainTexture1.texturingMode = LBTerrainTexture.TexturingMode.ConstantInfluence;
				lbTerrainTexture1.isCurvatureConcave = false;
				lbTerrainTexture1.curvatureDistance = 5f;
				lbTerrainTexture1.curvatureMinHeightDiff = 1f;
				lbTerrainTexture1.map = mapTex1;
				lbTerrainTexture1.mapColour = new Color(1f, 0f, 0f, 1f);
				lbTerrainTexture1.isDisabled = false;
				lbTerrainTexture1.mapTolerance = 1;
				lbTerrainTexture1.useNoise = true;
				lbTerrainTexture1.noiseTileSize = 100f;
				lbTerrainTexture1.isMinimalBlendingEnabled = false;
				lbTerrainTexture1.mapInverse = false;
				lbTerrainTexture1.useAdvancedMapTolerance = false;
				lbTerrainTexture1.mapToleranceRed = 0;
				lbTerrainTexture1.mapToleranceGreen = 0;
				lbTerrainTexture1.mapToleranceBlue = 0;
				lbTerrainTexture1.mapToleranceAlpha = 0;
				lbTerrainTexture1.mapWeightRed = 1f;
				lbTerrainTexture1.mapWeightGreen = 1f;
				lbTerrainTexture1.mapWeightBlue = 1f;
				lbTerrainTexture1.mapWeightAlpha = 1f;
				lbTerrainTexture1.mapToleranceBlendCurvePreset = LBCurve.BlendCurvePreset.Cubed;
				lbTerrainTexture1.mapToleranceBlendCurve = mapToleranceBlendCurveTex1;
				lbTerrainTexture1.mapIsPath = false;
				lbTerrainTexture1.isTinted = false;
				lbTerrainTexture1.tintColour = new Color(0f, 0f, 0f, 0f);
				lbTerrainTexture1.tintStrength = 0.5f;
				lbTerrainTexture1.tintedTexture = null;
				lbTerrainTexture1.isRotated = false;
				lbTerrainTexture1.rotationAngle = 0f;
				lbTerrainTexture1.rotatedTexture = null;
				lbTerrainTexture1.showTexture = true;
				lbTerrainTexture1.noiseOffset = 41.58604f;
				lbTerrainTexture1.GUID = "b3bace9c-fc18-44e1-9770-f3c42fffe664";
				lbTerrainTexture1.blendCurveMode = LBTerrainTexture.BlendCurveMode.BlendMinMaxValues;
				lbTerrainTexture1.filterList = new List<LBTextureFilter>();
				lbTerrainTexture1.lbTerrainDataList = null;
				// NOTE Add the new Texture to the landscape meta-data
				landscape.terrainTexturesList.Add(lbTerrainTexture1);
			}
			#endregion

			#region LBTerrainTexture2
			AnimationCurve mapToleranceBlendCurveTex2 = new AnimationCurve();
			mapToleranceBlendCurveTex2.AddKey(0.00f, 0.00f);
			mapToleranceBlendCurveTex2.AddKey(0.50f, 0.13f);
			mapToleranceBlendCurveTex2.AddKey(1.00f, 1.00f);
			Keyframe[] mapToleranceBlendCurveTex2Keys = mapToleranceBlendCurveTex2.keys;
			mapToleranceBlendCurveTex2Keys[0].inTangent = 0.00f;
			mapToleranceBlendCurveTex2Keys[0].outTangent = 0.00f;
			mapToleranceBlendCurveTex2Keys[1].inTangent = 0.75f;
			mapToleranceBlendCurveTex2Keys[1].outTangent = 0.75f;
			mapToleranceBlendCurveTex2Keys[2].inTangent = 3.00f;
			mapToleranceBlendCurveTex2Keys[2].outTangent = 3.00f;
			mapToleranceBlendCurveTex2 = new AnimationCurve(mapToleranceBlendCurveTex2Keys);

			LBTerrainTexture lbTerrainTexture2 = new LBTerrainTexture();
			if (lbTerrainTexture2 != null)
			{
				lbTerrainTexture2.texture = textureTex2;
				lbTerrainTexture2.normalMap = normalMapTex2;
				lbTerrainTexture2.heightMap = heightMapTex2;
				lbTerrainTexture2.textureName = "Grass (Hill)";
				lbTerrainTexture2.normalMapName = "GrassMeadowsNM";
				lbTerrainTexture2.tileSize = new Vector2(25, 25);
				lbTerrainTexture2.smoothness = 0f;
				lbTerrainTexture2.metallic = 0f;
				lbTerrainTexture2.minHeight = 0.05f;
				lbTerrainTexture2.maxHeight = 0.2666667f;
				lbTerrainTexture2.minInclination = 0f;
				lbTerrainTexture2.maxInclination = 30f;
				lbTerrainTexture2.strength = 1f;
				lbTerrainTexture2.texturingMode = LBTerrainTexture.TexturingMode.HeightAndInclination;
				lbTerrainTexture2.isCurvatureConcave = false;
				lbTerrainTexture2.curvatureDistance = 5f;
				lbTerrainTexture2.curvatureMinHeightDiff = 1f;
				lbTerrainTexture2.map = mapTex2;
				lbTerrainTexture2.mapColour = new Color(1f, 0f, 0f, 1f);
				lbTerrainTexture2.isDisabled = false;
				lbTerrainTexture2.mapTolerance = 1;
				lbTerrainTexture2.useNoise = true;
				lbTerrainTexture2.noiseTileSize = 100f;
				lbTerrainTexture2.isMinimalBlendingEnabled = false;
				lbTerrainTexture2.mapInverse = false;
				lbTerrainTexture2.useAdvancedMapTolerance = false;
				lbTerrainTexture2.mapToleranceRed = 0;
				lbTerrainTexture2.mapToleranceGreen = 0;
				lbTerrainTexture2.mapToleranceBlue = 0;
				lbTerrainTexture2.mapToleranceAlpha = 0;
				lbTerrainTexture2.mapWeightRed = 1f;
				lbTerrainTexture2.mapWeightGreen = 1f;
				lbTerrainTexture2.mapWeightBlue = 1f;
				lbTerrainTexture2.mapWeightAlpha = 1f;
				lbTerrainTexture2.mapToleranceBlendCurvePreset = LBCurve.BlendCurvePreset.Cubed;
				lbTerrainTexture2.mapToleranceBlendCurve = mapToleranceBlendCurveTex2;
				lbTerrainTexture2.mapIsPath = false;
				lbTerrainTexture2.isTinted = false;
				lbTerrainTexture2.tintColour = new Color(0f, 0f, 0f, 0f);
				lbTerrainTexture2.tintStrength = 0.5f;
				lbTerrainTexture2.tintedTexture = null;
				lbTerrainTexture2.isRotated = false;
				lbTerrainTexture2.rotationAngle = 0f;
				lbTerrainTexture2.rotatedTexture = null;
				lbTerrainTexture2.showTexture = true;
				lbTerrainTexture2.noiseOffset = 41.59176f;
				lbTerrainTexture2.GUID = "8c236d2c-6abe-453e-a3b1-49a274e220d1";
				lbTerrainTexture2.blendCurveMode = LBTerrainTexture.BlendCurveMode.BlendMinMaxValues;
				lbTerrainTexture2.filterList = new List<LBTextureFilter>();
				lbTerrainTexture2.lbTerrainDataList = null;
				// NOTE Add the new Texture to the landscape meta-data
				landscape.terrainTexturesList.Add(lbTerrainTexture2);
			}
			#endregion

			// Texture the terrains
			landscape.ApplyTextures(true, showErrors);

			// Paste code generated from Grass Tab items here

			// Add Grass to the terrains
			landscape.ApplyGrass(true, true);

			// Paste code generated from Groups Tab items here
			#region Group1 [CottagePath]
			LBGroup lbGroup1 = new LBGroup();
			if (lbGroup1 != null)
			{
				#region Group1-level variables
				lbGroup1.groupName = "CottagePath";
				lbGroup1.GUID = "ff70681f-c563-4159-aed9-96f5462baae5";
				lbGroup1.lbGroupType = LBGroup.LBGroupType.Uniform;
				lbGroup1.maxGroupSqrKm = 10;
				lbGroup1.isDisabled = false;
				lbGroup1.showInEditor = true;
				lbGroup1.showGroupDefaultsInEditor = true;
				lbGroup1.showGroupOptionsInEditor = true;
				lbGroup1.showGroupMembersInEditor = true;
				lbGroup1.showGroupDesigner = false;
				lbGroup1.showGroupsInScene = false;
				lbGroup1.showtabInEditor = 0;
				lbGroup1.isMemberListExpanded = true;
				// Clearing Group variables
				lbGroup1.minClearingRadius = 100f;
				lbGroup1.maxClearingRadius = 100f;
				lbGroup1.isFixedRotation = false;
				lbGroup1.startClearingRotationY = 0f;
				lbGroup1.endClearingRotationY = 359.9f;
				lbGroup1.isRemoveExistingGrass = true;
				lbGroup1.removeExistingGrassBlendDist = 0.5f;
				lbGroup1.isRemoveExistingTrees = true;
				// Proximity variables
				lbGroup1.proximityExtent = 10f;
				// Default values per group
				lbGroup1.minScale = 1f;
				lbGroup1.maxScale = 1f;
				lbGroup1.minHeight = 0f;
				lbGroup1.maxHeight = 1f;
				lbGroup1.minInclination = 0f;
				lbGroup1.maxInclination = 90f;
				// Group flatten terrain variables
				lbGroup1.isTerrainFlattened = false;
				lbGroup1.flattenHeightOffset = 0f;
				lbGroup1.flattenBlendRate = 0.5f;
				#endregion

				#region Group1 Zones
				// Start Group1-Level Zones
				// End Group1-Level Zones
				#endregion

				#region Group1 Filters
				lbGroup1.filterList = new List<LBFilter>();
				lbGroup1.isClearingRadiusFiltered = false;
				#endregion

				#region Group1 Textures
				// Only apply to Procedural and Manual Clearing groups
				lbGroup1.textureList = new List<LBGroupTexture>();
				#endregion

				#region Group1 Grass
				// Only apply to Procedural and Manual Clearing groups
				lbGroup1.grassList = new List<LBGroupGrass>();
				#endregion

				// Start Group Members
				#region Group1 Member1 
				LBGroupMember group1_lbGroupMember1 = new LBGroupMember();
				if (group1_lbGroupMember1 != null)
				{
					group1_lbGroupMember1.isDisabled = false;
					group1_lbGroupMember1.showInEditor = true;
					group1_lbGroupMember1.showtabInEditor = 2;
					group1_lbGroupMember1.GUID = "1d21f62f-395e-4a4f-a0db-a84138df01bc";
					group1_lbGroupMember1.lbMemberType = LBGroupMember.LBMemberType.ObjPath;
					group1_lbGroupMember1.isGroupOverride = false;
					group1_lbGroupMember1.minScale = 1f;
					group1_lbGroupMember1.maxScale = 1f;
					group1_lbGroupMember1.minHeight = 0f;
					group1_lbGroupMember1.maxHeight = 1f;
					group1_lbGroupMember1.minInclination = 0f;
					group1_lbGroupMember1.maxInclination = 90f;
					group1_lbGroupMember1.prefab = null;
					group1_lbGroupMember1.prefabName = "";
					group1_lbGroupMember1.showPrefabPreview = false;
					group1_lbGroupMember1.isKeepPrefabConnection = false;
					group1_lbGroupMember1.isCombineMesh = false;
					group1_lbGroupMember1.isRemoveEmptyGameObjects = true;
					group1_lbGroupMember1.isRemoveAnimator = true;
					group1_lbGroupMember1.isCreateCollider = false;
					group1_lbGroupMember1.maxPrefabSqrKm = 10;
					group1_lbGroupMember1.maxPrefabPerGroup = 10000;
					group1_lbGroupMember1.isPlacedInCentre = false;
					group1_lbGroupMember1.showXYZSettings = false;
					group1_lbGroupMember1.modelOffsetX = 0f;
					group1_lbGroupMember1.modelOffsetY = 0f;
					group1_lbGroupMember1.modelOffsetZ = 0f;
					group1_lbGroupMember1.minOffsetX = 0f;
					group1_lbGroupMember1.minOffsetZ = 0f;
					group1_lbGroupMember1.minOffsetY = 0f;
					group1_lbGroupMember1.maxOffsetY = 0f;
					group1_lbGroupMember1.randomiseOffsetY = false;
					group1_lbGroupMember1.rotationType = LBGroupMember.LBRotationType.WorldSpace;
					group1_lbGroupMember1.randomiseRotationY = true;
					group1_lbGroupMember1.startRotationY = 0f;
					group1_lbGroupMember1.endRotationY = 359.9f;
					group1_lbGroupMember1.randomiseRotationXZ = false;
					group1_lbGroupMember1.rotationX = 0f;
					group1_lbGroupMember1.endRotationX = 0f;
					group1_lbGroupMember1.rotationZ = 0f;
					group1_lbGroupMember1.endRotationZ = 0f;
					group1_lbGroupMember1.isLockTilt = false;
					group1_lbGroupMember1.useNoise = false;
					group1_lbGroupMember1.noiseOffset = 207.9302f;
					group1_lbGroupMember1.noiseTileSize = 500f;
					group1_lbGroupMember1.noisePlacementCutoff = 1f;
					group1_lbGroupMember1.proximityExtent = 10f;
					group1_lbGroupMember1.removeGrassBlendDist = 0.5f;
					group1_lbGroupMember1.minGrassProximity = 0f;
					group1_lbGroupMember1.isRemoveTree = true;
					group1_lbGroupMember1.minTreeProximity = 10f;
					group1_lbGroupMember1.isTerrainAligned = false;
					group1_lbGroupMember1.isTerrainFlattened = false;
					group1_lbGroupMember1.flattenDistance = 2f;
					group1_lbGroupMember1.flattenHeightOffset = 0f;
					group1_lbGroupMember1.flattenBlendRate = 0.5f;

					// Start Member-Level Zones references for group1_lbGroupMember1

					// End Member-Level Zones references for group1_lbGroupMember1

					group1_lbGroupMember1.isZoneEdgeFillTop = false;
					group1_lbGroupMember1.isZoneEdgeFillBottom = false;
					group1_lbGroupMember1.isZoneEdgeFillLeft = false;
					group1_lbGroupMember1.isZoneEdgeFillRight = false;
					group1_lbGroupMember1.zoneEdgeFillDistance = 1f;
					group1_lbGroupMember1.isPathOnly = false;
					group1_lbGroupMember1.usePathHeight = false;
					group1_lbGroupMember1.usePathSlope = false;
					group1_lbGroupMember1.useTerrainTrend = false;
					group1_lbGroupMember1.lbObjectOrientation = LBObjPath.LBObjectOrientation.PathSpace;

					#region Start Object Path settings for [CottageObjectPath1]
					group1_lbGroupMember1.lbObjPath = new LBObjPath();
					if (group1_lbGroupMember1.lbObjPath != null)
					{
						// LBPath settings
						group1_lbGroupMember1.lbObjPath.pathName = "CottageObjectPath1";
						group1_lbGroupMember1.lbObjPath.showPathInScene = false;
						group1_lbGroupMember1.lbObjPath.blendStart = true;
						group1_lbGroupMember1.lbObjPath.blendEnd = true;
						group1_lbGroupMember1.lbObjPath.pathResolution = 2f;
						group1_lbGroupMember1.lbObjPath.closedCircuit = false;
						group1_lbGroupMember1.lbObjPath.edgeBlendWidth = 5f;
						group1_lbGroupMember1.lbObjPath.removeCentre = false;
						group1_lbGroupMember1.lbObjPath.leftBorderWidth = 0.5f;
						group1_lbGroupMember1.lbObjPath.rightBorderWidth = 0.5f;
						group1_lbGroupMember1.lbObjPath.snapToTerrain = false;
						group1_lbGroupMember1.lbObjPath.heightAboveTerrain = 2f;
						group1_lbGroupMember1.lbObjPath.zoomOnFind = true;
						group1_lbGroupMember1.lbObjPath.findZoomDistance = 50f;
						// LBPath Surface options
						group1_lbGroupMember1.lbObjPath.isMeshLandscapeUV = false;
						group1_lbGroupMember1.lbObjPath.meshUVTileScale = new Vector2(1f, 1f);
						group1_lbGroupMember1.lbObjPath.meshYOffset = 0f;
						group1_lbGroupMember1.lbObjPath.meshEdgeSnapToTerrain = false;
						group1_lbGroupMember1.lbObjPath.meshSnapType = LBPath.MeshSnapType.BothEdges;
						group1_lbGroupMember1.lbObjPath.meshIsDoubleSided = false;
						group1_lbGroupMember1.lbObjPath.meshIncludeEdges = true;
						group1_lbGroupMember1.lbObjPath.meshIncludeWater = false;
						group1_lbGroupMember1.lbObjPath.meshSaveToProjectFolder = false;
						// Path Points - SEE BELOW - will be imported from JSON files

						//group1_lbGroupMember1.lbObjPath.minPathWidth = group1_lbGroupMember1.lbObjPath.GetMinWidth();
						// LBObjPath settings
						group1_lbGroupMember1.lbObjPath.useWidth = true;
						group1_lbGroupMember1.lbObjPath.useSurfaceMesh = false;
						group1_lbGroupMember1.lbObjPath.isCreateSurfaceMeshCollider = false;
						group1_lbGroupMember1.lbObjPath.layoutMethod = LBObjPath.LayoutMethod.Spacing;
						group1_lbGroupMember1.lbObjPath.selectionMethod = LBObjPath.SelectionMethod.Alternating;
						group1_lbGroupMember1.lbObjPath.spacingDistance = 10f;
						group1_lbGroupMember1.lbObjPath.maxMainPrefabs = 5;
						group1_lbGroupMember1.lbObjPath.isLastObjSnappedToEnd = true;
						group1_lbGroupMember1.lbObjPath.isRandomisePerGroupRegion = false;
						group1_lbGroupMember1.lbObjPath.surroundSmoothing = 0f;
						group1_lbGroupMember1.lbObjPath.addTerrainHeight = 0f;
						group1_lbGroupMember1.lbObjPath.isSwitchMeshUVs = false;
						group1_lbGroupMember1.lbObjPath.isSwitchBaseMeshUVs = false;
						group1_lbGroupMember1.lbObjPath.baseMeshThickness = 0f;
						group1_lbGroupMember1.lbObjPath.baseMeshUVTileScale = new Vector2(1f, 1f);
						group1_lbGroupMember1.lbObjPath.baseMeshUseIndent = false;
						group1_lbGroupMember1.lbObjPath.isCreateBaseMeshCollider = false;
						group1_lbGroupMember1.lbObjPath.coreTextureGUID = "";
						group1_lbGroupMember1.lbObjPath.coreTextureNoiseTileSize = 0f;
						group1_lbGroupMember1.lbObjPath.coreTextureStrength = 1f;
						group1_lbGroupMember1.lbObjPath.surroundTextureGUID = "";
						group1_lbGroupMember1.lbObjPath.surroundTextureNoiseTileSize = 0f;
						group1_lbGroupMember1.lbObjPath.surroundTextureStrength = 1f;
						group1_lbGroupMember1.lbObjPath.isRemoveExistingGrass = true;
						group1_lbGroupMember1.lbObjPath.isRemoveExistingTrees = false;
						group1_lbGroupMember1.lbObjPath.treeDistFromEdge = 0f;
						group1_lbGroupMember1.lbObjPath.useBiomes = false;
						group1_lbGroupMember1.lbObjPath.surfaceMeshMaterial = null;
						group1_lbGroupMember1.lbObjPath.baseMeshMaterial = null;
						// ObjPath Curves
						AnimationCurve grp1_gmbr1profileHeightCurve = new AnimationCurve();
						grp1_gmbr1profileHeightCurve.AddKey(0.00f, 0.50f);
						grp1_gmbr1profileHeightCurve.AddKey(1.00f, 0.50f);
						Keyframe[] grp1_gmbr1profileHeightCurveKeys = grp1_gmbr1profileHeightCurve.keys;
						grp1_gmbr1profileHeightCurveKeys[0].inTangent = 0.00f;
						grp1_gmbr1profileHeightCurveKeys[0].outTangent = 0.00f;
						grp1_gmbr1profileHeightCurveKeys[1].inTangent = 0.00f;
						grp1_gmbr1profileHeightCurveKeys[1].outTangent = 0.00f;
						grp1_gmbr1profileHeightCurve = new AnimationCurve(grp1_gmbr1profileHeightCurveKeys);

						AnimationCurve grp1_gmbr1surroundBlendCurve = new AnimationCurve();
						grp1_gmbr1surroundBlendCurve.AddKey(0.00f, 0.00f);
						grp1_gmbr1surroundBlendCurve.AddKey(1.00f, 1.00f);
						Keyframe[] grp1_gmbr1surroundBlendCurveKeys = grp1_gmbr1surroundBlendCurve.keys;
						grp1_gmbr1surroundBlendCurveKeys[0].inTangent = 0.00f;
						grp1_gmbr1surroundBlendCurveKeys[0].outTangent = 0.00f;
						grp1_gmbr1surroundBlendCurveKeys[1].inTangent = 0.00f;
						grp1_gmbr1surroundBlendCurveKeys[1].outTangent = 0.00f;
						grp1_gmbr1surroundBlendCurve = new AnimationCurve(grp1_gmbr1surroundBlendCurveKeys);

						group1_lbGroupMember1.lbObjPath.profileHeightCurve = grp1_gmbr1profileHeightCurve;
						group1_lbGroupMember1.lbObjPath.surroundBlendCurve = grp1_gmbr1surroundBlendCurve;
						group1_lbGroupMember1.lbObjPath.profileHeightCurvePreset = LBCurve.ObjPathHeightCurvePreset.Flat;

						group1_lbGroupMember1.lbObjPath.pathPointList = new List<LBPathPoint>();
						group1_lbGroupMember1.lbObjPath.mainObjPrefabList = new List<LBObjPrefab>();
						// ObjPath Points - SEE BELOW - will be imported from JSON files

						// Start ObjPrefab
						group1_lbGroupMember1.lbObjPath.startObjPrefab = new LBObjPrefab();
						group1_lbGroupMember1.lbObjPath.startObjPrefab.groupMemberGUID = "";
						// End ObjPrefab
						group1_lbGroupMember1.lbObjPath.endObjPrefab = new LBObjPrefab();
						group1_lbGroupMember1.lbObjPath.endObjPrefab.groupMemberGUID = "";
						group1_lbGroupMember1.lbObjPath.lbObjPathSeriesList = new List<LBObjPathSeries>();
						group1_lbGroupMember1.lbObjPath.isSeriesListOverride = false;
						group1_lbGroupMember1.lbObjPath.seriesListGroupMemberGUID = "";
					}
					#endregion End Object Path settings for [CottageObjectPath1]
				}
				lbGroup1.groupMemberList.Add(group1_lbGroupMember1);
				#endregion
				// End Group Members

				// NOTE Add the new Group to the landscape meta-data
				landscape.lbGroupList.Add(lbGroup1);
			}
			#endregion

			// In this example, we will import a Group Object Path points from a JSON file.
			#region Import Object Path points from JSON

			if (!string.IsNullOrEmpty(objPathGUID))
			{
				// Find the object path (there are several ways you could do this)
				lbObjPath = LBGroup.GetObjectPath(landscape.GroupList(), objPathGUID);

				if (lbObjPath != null)
				{
					// Append the .json extension
					if (!string.IsNullOrEmpty(grp1Path1PointsFile) && !grp1Path1PointsFile.EndsWith(".json"))
					{
						grp1Path1PointsFile += ".json";
					}

					// Attempt to import the path points
					landscape.ImportPathPointFromJson(lbObjPath, Application.dataPath + "/" + folderJSON, grp1Path1PointsFile);

					if (isSnapPathToTerrain)
					{
						lbObjPath.snapToTerrain = true;
						lbObjPath.RefreshPathHeights(landscape, false);

						// Force refresh of spline cache
						lbObjPath.isSplinesCached2 = false;
						lbObjPath.RefreshObjPathPositions(lbObjPath.showSurroundingInScene, false);
					}
				}
			}
			#endregion

			#region Camera Animator
			// Get the first Camera Animator in the scene, snap the camera path to the new terrain,
			// and start moving the camera along the camera path.
			LBCameraAnimator lbCameraAnimator = LBCameraAnimator.GetFirstCameraAnimatorInLandscape(landscape);
			if (lbCameraAnimator == null)
			{
				#if UNITY_EDITOR
				Debug.LogWarning("GetFirstCameraAnimatorInLandscape returned null");
				#endif
			}
			else
			{
				if (!string.IsNullOrEmpty(objPathGUID))
				{
					if (lbObjPath != null)
					{
						if (!lbCameraAnimator.cameraPath.ImportPathPoints(lbObjPath))
						{
							#if UNITY_EDITOR
							Debug.LogWarning("Could not make camera animator travel along new Group Object Path.");
							#endif
						}
						else
						{
							// Start from the other end of the path
							lbCameraAnimator.ReversePath();
						}
					}
					else if (showErrors) { Debug.LogWarning("Could not make camera animator travel along new Group Object Path because the GUID supplied did not match an existing Group Object Path"); }
				}
				else if (showErrors) { Debug.LogWarning("Could not make camera animator travel along new Group Object Path because the GUID is set in the CameraPath script public variables"); }

				// Get the LBPath instance which contains the points along the camera path
				LBPath lbPath = lbCameraAnimator.cameraPath.lbPath;
				if (lbPath == null) { Debug.LogWarning("Could not find the camera path instance for the animator"); }
				else
				{
					// Optionally update the path points to match the terrain
					lbPath.heightAboveTerrain = 3f;
					lbPath.snapToTerrain = true;
					lbPath.RefreshPathHeights(landscape);

					lbCameraAnimator.minMoveSpeed = 2f;
					lbCameraAnimator.maxMoveSpeed = 6f;
					lbCameraAnimator.animateSpeed = true;
					lbCameraAnimator.pauseAtEndDuration = 300f;

					// Start the camera moving from the start of the path.
					lbCameraAnimator.BeginAnimation(true, 0f);
                }
			}
			#endregion

			landscape.ApplyGroups(false, false);

			// Display the total time taken to generate the landscape (usually for debugging purposes)
			Debug.Log("Time taken to generate landscape: " + (Time.realtimeSinceStartup - generationStartTime).ToString("00.00") + " seconds.");
		}
	}
}
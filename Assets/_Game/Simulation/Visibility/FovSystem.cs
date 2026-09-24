using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

namespace Zomboid.Simulation.Visibility
{
    [ExecuteAlways]
    public class FovSystem : MonoBehaviour
    {
        public Transform Player;
        public float OrthoSize = 30f;
        public Color FogColor = new Color(0.05f, 0.05f, 0.05f, 1f);

        private Camera _maskCam;
        private RenderTexture _maskRT;
        private CustomPassVolume _volume;
        private Material _fullscreenMat;
        public float AwareRadius = 2f;
        public float ViewRadius = 25f;
        public float ViewAngle = 120f;

        public static FovSystem Instance;
        public Camera MaskCamera => _maskCam;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            Instance = this;

            // CustomPassVolume ниже — isGlobal, красит ВСЕ камеры сцены, включая Scene View редактора.
            // Поэтому не создаём камеру/маску/volume вне Play — иначе весь Scene view темнеет при [ExecuteAlways].
            if (!Application.isPlaying) return;

            var oldVolumes = GetComponents<CustomPassVolume>();
            foreach(var v in oldVolumes) DestroyImmediate(v);
            
            var oldCameras = GetComponentsInChildren<Camera>();
            foreach(var c in oldCameras) DestroyImmediate(c.gameObject);

            _maskRT = new RenderTexture(1024, 1024, 0, RenderTextureFormat.R8);
            _maskRT.name = "FovMaskRT";
            _maskRT.Create();

            var camGo = new GameObject("FovMaskCamera");
            camGo.transform.SetParent(transform);
            camGo.transform.eulerAngles = new Vector3(90, 0, 0);
            
            _maskCam = camGo.AddComponent<Camera>();
            _maskCam.orthographic = true;
            _maskCam.orthographicSize = OrthoSize;
            _maskCam.clearFlags = CameraClearFlags.SolidColor;
            _maskCam.backgroundColor = Color.black;
            _maskCam.cullingMask = 1 << 31;
            _maskCam.targetTexture = _maskRT;
            _maskCam.enabled = true; 

            var hdCam = camGo.AddComponent<HDAdditionalCameraData>();
            hdCam.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
            hdCam.backgroundColorHDR = Color.black;
            hdCam.volumeLayerMask = 0;
            
            hdCam.customRenderingSettings = true;
            hdCam.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.Postprocess] = true;
            hdCam.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Postprocess, false);
            hdCam.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.AtmosphericScattering] = true;
            hdCam.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.AtmosphericScattering, false);
            hdCam.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.CustomPass] = true;
            hdCam.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.CustomPass, false);

            _volume = gameObject.AddComponent<CustomPassVolume>();
            _volume.isGlobal = true;
            _volume.injectionPoint = CustomPassInjectionPoint.BeforePostProcess;

            Shader shader = Shader.Find("FullScreen/FovDarken");
#if UNITY_EDITOR
            if (shader == null) shader = UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Game/Simulation/Visibility/FovDarken.shader");
#endif
            if (shader != null)
            {
                _fullscreenMat = new Material(shader);
                _fullscreenMat.SetTexture("_FovMask", _maskRT);
                _fullscreenMat.SetVector("_FovParams", new Vector4(0, 0, OrthoSize > 0 ? OrthoSize : 30f, 0));
                _fullscreenMat.SetColor("_DarkColor", FogColor);
                
                float cosHalfAngle = Mathf.Cos(ViewAngle * 0.5f * Mathf.Deg2Rad);
                _fullscreenMat.SetVector("_VisAware", new Vector4(AwareRadius, ViewRadius, cosHalfAngle, 0));
                _fullscreenMat.SetVector("_VisForward", new Vector4(0, 1, 0, 0));

                var pass = new FullScreenCustomPass
                {
                    name = "FovDarkenPass",
                    fullscreenPassMaterial = _fullscreenMat,
                    fetchColorBuffer = false
                };
                _volume.customPasses.Add(pass);
            }
        }

        private void OnDisable()
        {
            if (_maskCam != null) DestroyImmediate(_maskCam.gameObject);
            if (_maskRT != null) _maskRT.Release();
            if (_fullscreenMat != null) DestroyImmediate(_fullscreenMat);
            if (_volume != null) DestroyImmediate(_volume);
        }

        public void SetPlayer(Transform player)
        {
            Player = player;
            if (VisibilitySolver.Instance != null)
            {
                VisibilitySolver.Instance.SetPlayer(player);
            }
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying) return;
            if (Player == null || _maskCam == null || _fullscreenMat == null) return;

            _fullscreenMat.SetColor("_DarkColor", FogColor);

            float cosHalfAngle = Mathf.Cos(ViewAngle * 0.5f * Mathf.Deg2Rad);
            _fullscreenMat.SetVector("_VisAware", new Vector4(AwareRadius, ViewRadius, cosHalfAngle, 0));
            _fullscreenMat.SetVector("_VisForward", new Vector4(Player.forward.x, Player.forward.z, 0, 0));

            _maskCam.transform.position = new Vector3(Player.position.x, Player.position.y + 50f, Player.position.z);
            
            _fullscreenMat.SetTexture("_FovMask", _maskRT);
            _fullscreenMat.SetVector("_FovParams", new Vector4(Player.position.x, Player.position.z, OrthoSize, 0));
        }
    }
}

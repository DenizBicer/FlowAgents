using UnityEngine;
    public class PerlinUpdater : MonoBehaviour
    {
        [SerializeField] CustomRenderTexture _texture;
        [SerializeField, Range(1, 16)] int _stepsPerFrame = 4;

        void Start()
        {
            _texture.Initialize();
        }

        void Update()
        {
            _texture.Update(_stepsPerFrame);
        }
    }


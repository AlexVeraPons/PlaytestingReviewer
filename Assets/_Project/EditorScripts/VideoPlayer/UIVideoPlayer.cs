using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UIElements.Image;

namespace PlaytestingReviewer.Video
{
    [UxmlElement]
    public partial class UIVideoPlayer : VisualElement, IVideoPlayer
    {
        // -------------------------------------------------- fields
        UnityEngine.Video.VideoPlayer _player;
        Image _target;
        RenderTexture _rt;
        float _pausedTime;

        // -------------------------------------------------- ctor
        public UIVideoPlayer()
        {
            style.flexDirection = FlexDirection.Row;
            style.flexGrow = 1;

            var container = new VisualElement
            {
                style = { flexGrow = 1, alignItems = Align.FlexStart }
            };
            Add(container);

            _target = new Image();
            container.Add(_target);

            InitPlayer();

            RegisterCallback<DetachFromPanelEvent>(_ => DisposePlayer());
        }

        // -------------------------------------------------- create VideoPlayer
        void InitPlayer()
        {
            var go = new GameObject("RuntimeVideoPlayer")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _player = go.AddComponent<UnityEngine.Video.VideoPlayer>();
            _player.playOnAwake = false;
            _player.isLooping = false;

            _rt = new RenderTexture(1920, 1080, 0);
            _player.targetTexture = _rt;

            /* ---------- update route ----------- */
#if UNITY_EDITOR
            // In the editor we can reuse the global update loop
            UnityEditor.EditorApplication.update += UpdateFrame_Editor;
#else
            // In player builds use frameReady — no Editor API needed
            _player.sendFrameReadyEvents = true;
            _player.frameReady += (_, __) => PushTexture();
#endif
        }

        // -------------------------------------------------- IVideoPlayer impl
        public void SetVideo(string path) => _player.url = "file://" + path;

        public void Play()
        {
            if (string.IsNullOrEmpty(_player.url)) return;
            if (_player.isPlaying) return;

            _player.time = _pausedTime;
            _player.Play();
        }

        public void Pause()
        {
            if (!_player.isPlaying) return;
            _player.Pause();
            _pausedTime = (float)_player.time;
            PushTexture();
        }

        public void Stop() => _player.Stop();
        public bool IsPlaying() => _player.isPlaying;

        public float GetVideoLengthSeconds()
        {
            if (_player == null) return 0;
            
            return (float)_player.length;
        }

        public void NextFrame(int n = 1)
        {
            SeekRelative(n);
        }

        public void PreviousFrame(int n = 1)
        {
            SeekRelative(-n);
        }

        public void GoToStart()
        {
            _player.frame = 0;
        }

        public void GoToEnd()
        {
            _player.frame = (long)_player.frameCount - 1;
        }

        public float GetVideoLength() => (float)_player.length;
        public int GetVideoLengthFrames() => (int)_player.frameCount;
        public float GetCurrentTime() => (float)_player.time;
        public int GetCurrentFrame() => (int)_player.frame;

        public void SetFrame(int f)
        {
            if (f < 0 || f >= (int)_player.frameCount) return;
            _player.frame = f;
            _pausedTime = (float)f / _player.frameRate;
            PushTexture();
        }

        public string GetVideoPath() => _player.url;

#if UNITY_EDITOR
        void UpdateFrame_Editor()
        {
            if (_player == null || !_player.isPrepared) return;
            if (_player.isPlaying) PushTexture();
        }
#endif

        void PushTexture()
        {
            if (_player.texture == null) return;
            MarkDirtyRepaint();
            _target.image = _player.texture;
        }

        void SeekRelative(int delta)
        {
            if (_player.isPlaying) return;

            long newFrame = _player.frame + delta;
            newFrame = (long)Mathf.Clamp(newFrame, 0, _player.frameCount - 1);
            _player.frame = newFrame;
            PushTexture();
        }

        // -------------------------------------------------- clean-up
        void DisposePlayer()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= UpdateFrame_Editor;
#endif
            if (_rt != null) Object.DestroyImmediate(_rt);
            if (_player != null) Object.DestroyImmediate(_player.gameObject);
        }
    }
}
using System.Collections;
using Code.Agents;
using Code.Agents.Characters;
using Code.CoreSystem.Bus;
using Code.GameEvents.Worlds;
using Code.UIs;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Worlds
{
    /// <summary>
    /// Core 씬에 두는 지역 전환 관리자입니다. Core는 계속 켜 두고, 지역 씬 하나만 Additive로 바꿔 끼웁니다.
    /// 플레이어 · 카메라 · UI는 Core에 있어서 지역을 옮겨도 그대로 이어집니다.
    /// </summary>
    public class AreaLoader : MonoBehaviour
    {
        [Header("Start Area")]
        [SerializeField] private AreaSO startArea;
        [SerializeField] private string startSpawnPointId = "Default";

        [Header("Respawn")]
        [SerializeField] private AreaSO respawnArea;
        [SerializeField] private string respawnSpawnPointId = "Default";
        [SerializeField, Min(0f)] private float respawnDelay = 2.5f; // 사망 모션을 보여 주는 시간

        [Header("References")]
        [SerializeField] private Character player;
        [SerializeField] private CinemachineCamera playerCamera;
        [SerializeField] private CharacterInputSO characterInput;
        [SerializeField] private ScreenFader screenFader;

        [Header("Event")]
        [SerializeField] private OwnerEnum eventOwner = OwnerEnum.Player;

        public AreaSO CurrentArea { get; private set; }    // 지금 있는 지역

        private BusSubscriptionScope _busScope;

        private CharacterController _characterController;
        private AgentMover _agentMover;
        private Scene _currentScene;
        private bool _isTransitioning;
        private bool _isGameCleared;

        private void Awake()
        {
            Debug.Assert(startArea != null, $"[AreaLoader] startArea 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(player != null, $"[AreaLoader] player 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(characterInput != null, $"[AreaLoader] characterInput 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(screenFader != null, $"[AreaLoader] screenFader 연결 안 됨: {gameObject.name}", this);
            Debug.Assert(respawnArea != null, $"[AreaLoader] respawnArea 연결 안 됨: {gameObject.name}", this);

            _characterController = player.GetComponent<CharacterController>();
            _agentMover = player.GetComponentInChildren<AgentMover>();

            _busScope = new BusSubscriptionScope();
            _busScope.Subscribe<AreaTransitionRequestEvent>(eventOwner, HandleTransitionRequest);
            _busScope.Subscribe<GameClearedEvent>(eventOwner, HandleGameClearedEvent);

            player.OnDeath.AddListener(HandlePlayerDeath);
        }

        private void Start()
        {
            screenFader.SetCovered(true);

            // 에디터에서 Core와 지역 씬을 같이 열고 플레이하면, 열려 있는 지역을 그대로 쓴다
            Scene openedArea = FindOpenedAreaScene();
            StartCoroutine(openedArea.IsValid()
                ? EnterOpenedAreaRoutine(openedArea)
                : TransitionRoutine(startArea, startSpawnPointId));
        }

        private void OnDestroy()
        {
            _busScope?.Dispose();
            player.OnDeath.RemoveListener(HandlePlayerDeath);
            if (characterInput != null)
                characterInput.SetGameplayBlock(this, false);
        }

        private void HandleTransitionRequest(AreaTransitionRequestEvent evt)
        {
            // 포탈 안에서 여러 번 닿거나 전환 중에 또 요청이 와도 한 번만 옮긴다
            if (_isTransitioning || evt.Area == null)
                return;

            StartCoroutine(TransitionRoutine(evt.Area, evt.SpawnPointId));
        }

        private void HandleGameClearedEvent(GameClearedEvent evt)
        {
            _isGameCleared = true;
        }

        private void HandlePlayerDeath()
        {
            if (_isGameCleared)
                return;

            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);

            if (_isGameCleared)
                yield break; // 리스폰 대기중 클리어

            // 포탈을 타는 중에 죽었다면 그 전환이 끝난 뒤에 옮긴다
            while (_isTransitioning)
                yield return null;

            yield return TransitionRoutine(respawnArea, respawnSpawnPointId, true);
        }

        private IEnumerator TransitionRoutine(AreaSO area, string spawnPointId, bool isRevivePlayer = false)
        {
            if (!Application.CanStreamedLevelBeLoaded(area.SceneName))
            {
                Debug.LogError($"[AreaLoader] 빌드 목록에 씬 {area.SceneName} 없음 → 이동하지 않음: {area.name}", this);
                yield break;
            }

            BeginTransition();
            yield return screenFader.FadeOut();

            // 풀에서 꺼낸 이펙트 · 드랍은 Core 씬에 있어 지역 씬과 같이 내려가지 않으므로, 화면이 가려진 뒤 회수시킨다
            Bus<AreaLeavingEvent>.Raise(eventOwner, new AreaLeavingEvent());

            if (_currentScene.IsValid() && _currentScene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(_currentScene);

            yield return SceneManager.LoadSceneAsync(area.SceneName, LoadSceneMode.Additive);
            _currentScene = SceneManager.GetSceneByName(area.SceneName);

            yield return EndTransition(area, spawnPointId, isRevivePlayer);
        }

        private IEnumerator EnterOpenedAreaRoutine(Scene scene)
        {
            BeginTransition();
            _currentScene = scene;

            // 열려 있는 씬이 시작 지역이면 데이터를 알 수 있고, 다른 지역이면 데이터 없이 들어간다
            AreaSO area = startArea != null && startArea.SceneName == scene.name ? startArea : null;
            yield return EndTransition(area, startSpawnPointId);
        }

        private void BeginTransition()
        {
            _isTransitioning = true;
            characterInput.SetGameplayBlock(this, true);

            // 새 지역 바닥이 올라오기 전에 중력으로 떨어지지 않게 이동을 멈춘다
            if (_agentMover != null)
            {
                _agentMover.StopImmediately();
                _agentMover.enabled = false;
            }
        }

        private IEnumerator EndTransition(AreaSO area, string spawnPointId, bool isRevivePlayer = false)
        {
            // 활성 씬이어야 지역 씬의 조명 · 안개 · 스카이박스가 적용되고, 런타임에 생기는 오브젝트도 그 지역 씬에 들어간다
            SceneManager.SetActiveScene(_currentScene);

            AreaSpawnPoint spawnPoint = FindSpawnPoint(_currentScene, spawnPointId);
            if (spawnPoint != null)
                WarpPlayer(spawnPoint.transform);

            // 화면이 가려진 동안 되살려야 마을에 쓰러져 있다 일어나는 모습이 안 보인다
            if (isRevivePlayer)
                player.Revive();

            if (_agentMover != null)
                _agentMover.enabled = true;

            CurrentArea = area;
            yield return screenFader.FadeIn();

            characterInput.SetGameplayBlock(this, false);
            _isTransitioning = false;

            if (area != null)
                Bus<AreaEnteredEvent>.Raise(eventOwner, new AreaEnteredEvent(area));
        }

        private void WarpPlayer(Transform destination)
        {
            Vector3 delta = destination.position - player.transform.position;

            // CharacterController가 켜져 있으면 다음 이동 때 예전 위치로 되돌아가므로 잠깐 끄고 옮긴다
            _characterController.enabled = false;
            player.transform.SetPositionAndRotation(destination.position,
                Quaternion.LookRotation(destination.forward, Vector3.up));

            _characterController.enabled = true;

            // 카메라가 먼 거리를 따라 쓸고 지나가지 않게 순간이동을 알린다
            if (playerCamera != null)
                playerCamera.OnTargetObjectWarped(player.transform, delta);
        }

        private Scene FindOpenedAreaScene()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene != gameObject.scene && scene.isLoaded)
                    return scene;
            }

            return default;
        }

        private static AreaSpawnPoint FindSpawnPoint(Scene scene, string spawnPointId)
        {
            AreaSpawnPoint fallback = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (AreaSpawnPoint spawnPoint in root.GetComponentsInChildren<AreaSpawnPoint>())
                {
                    if (spawnPoint.Id == spawnPointId)
                        return spawnPoint;

                    if (fallback == null)
                        fallback = spawnPoint;
                }
            }

            // 이름이 틀려도 지역 어딘가에는 내려 주고, 개발 중에 알 수 있게 경고한다
            Debug.Assert(fallback == null, $"[AreaLoader] 도착 지점 '{spawnPointId}' 없음 → 다른 지점 사용: {scene.name}");
            Debug.Assert(fallback != null, $"[AreaLoader] 도착 지점 하나도 없음: {scene.name}");
            return fallback;
        }
    }
}

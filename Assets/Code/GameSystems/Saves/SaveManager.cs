using System;
using System.Collections.Generic;
using System.IO;
using Code.CoreSystem.Bus;
using Code.GameEvents.Worlds;
using UnityEngine;

namespace Code.GameSystems.Saves
{
    /// <summary>
    /// Core 씬에 두는 저장 관리자입니다. 안전 지역(마을)에 들어오면 자동 저장하고, 게임을 시작할 때 불러옵니다.
    /// 씬에 있는 ISaveable을 모두 모아 아이디와 JSON 문자열 쌍으로 파일 하나에 씁니다.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        /// <summary>
        /// 저장 파일의 한 칸입니다. SaveId 번호와 그 주인이 만든 JSON 문자열을 담습니다.
        /// </summary>
        [Serializable]
        private struct SaveEntry
        {
            public int Id;
            public string Data;
        }

        /// <summary>
        /// 저장 파일 전체입니다.
        /// </summary>
        [Serializable]
        private struct SaveFile
        {
            public List<SaveEntry> Entries;
        }

        [Header("Save Setting")]
        [SerializeField] private string saveFileName = "save.json";
        [SerializeField] private bool isLoadOnStart = true;    // 끄면 저장 파일이 있어도 새 게임처럼 시작한다 (테스트용)
        [SerializeField] private bool isWaitForTitle;          // 켜면 타이틀에서 StartSession을 부르기 전까지 불러오지도 저장하지도 않는다

        [Header("Event")]
        [SerializeField] private OwnerEnum eventOwner = OwnerEnum.Player;

        // 지금 씬에 주인이 없는 데이터도 버리지 않고 다음 저장 때 그대로 다시 쓴다
        private readonly List<SaveEntry> _unusedEntries = new();
        private BusSubscriptionScope _busScope;
        // 타이틀에 있는 동안 마을 자동 저장 · 종료 저장이 빈 상태로 기존 저장을 덮어쓰지 않게 막는다
        private bool _isSaveEnabled;

        /// <summary> 이어할 저장 파일이 있는지 여부입니다. </summary>
        public bool HasSaveFile
            => File.Exists(SavePath);

        private string SavePath
            => Path.Combine(Application.persistentDataPath, saveFileName);

        private string TempPath
            => SavePath + ".tmp";

        private string CorruptPath
            => SavePath + ".corrupt";

        private void Awake()
        {
            _isSaveEnabled = !isWaitForTitle;
            _busScope = new BusSubscriptionScope();
            _busScope.Subscribe<AreaEnteredEvent>(eventOwner, HandleAreaEntered);
        }

        // 모든 Awake(모듈 초기화 · 시작 아이템 지급)가 끝난 뒤라 그 위에 덮어쓸 수 있다
        private void Start()
        {
            if (isLoadOnStart && !isWaitForTitle)
                Load();
        }

        private void OnDestroy()
        {
            _busScope?.Dispose();
        }

        // 필드에서 꺼도 진행이 남도록 종료 직전에 저장한다 (오브젝트 파괴 전이라 모듈 값을 읽을 수 있음)
        private void OnApplicationQuit()
        {
            Save();
        }

        // 모바일은 홈 버튼으로 내려간 뒤 OS가 그냥 꺼 버리면 OnApplicationQuit이 안 불리므로, 내려갈 때도 저장한다
        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
                Save();
        }

        private void HandleAreaEntered(AreaEnteredEvent evt)
        {
            if (evt.Area != null && evt.Area.IsSafeZone)
                Save();
        }

        /// <summary>
        /// 타이틀에서 게임을 시작할 때 부릅니다. 이어하기면 저장을 불러오고, 새 게임이면 저장을 지운 뒤 지금 상태로 새로 씁니다.
        /// 이때부터 자동 저장이 켜집니다.
        /// </summary>
        public void StartSession(bool isContinue)
        {
            if (isContinue)
                Load();
            else
                DeleteSave();

            _isSaveEnabled = true;
            Save();
        }

        /// <summary>
        /// 씬의 모든 ISaveable 데이터를 파일에 씁니다.
        /// </summary>
        public void Save()
        {
            if (!_isSaveEnabled)
                return;

            List<SaveEntry> entries = new();
            foreach (ISaveable saveable in FindSaveables())
                entries.Add(new SaveEntry { Id = saveable.SaveId.Id, Data = saveable.GetSaveData() });

            entries.AddRange(_unusedEntries);
            string json = JsonUtility.ToJson(new SaveFile { Entries = entries });

            // 바로 덮어쓰면 쓰는 도중 앱이 죽을 때 옛 저장까지 잃으므로, 임시 파일에 다 쓴 뒤 이름만 바꿔치기한다
            File.WriteAllText(TempPath, json);
            if (File.Exists(SavePath))
                File.Replace(TempPath, SavePath, null);
            else
                File.Move(TempPath, SavePath);
        }

        /// <summary>
        /// 저장 파일이 있으면 읽어서 각 ISaveable에 되돌립니다.
        /// </summary>
        public void Load()
        {
            if (!File.Exists(SavePath))
                return;

            // RestoreData 안의 진짜 버그까지 "깨진 파일"로 삼키지 않도록 파일을 읽는 부분만 감싼다
            SaveFile saveFile;
            try
            {
                saveFile = JsonUtility.FromJson<SaveFile>(File.ReadAllText(SavePath));
            }
            catch (Exception e) when (e is IOException or ArgumentException)
            {
                MoveBrokenSave(e.Message);
                return;
            }

            // 0바이트 파일은 예외 없이 빈 값으로 읽힌다
            if (saveFile.Entries == null)
            {
                MoveBrokenSave("저장 내용이 비어 있습니다.");
                return;
            }

            Dictionary<int, ISaveable> saveables = new();
            foreach (ISaveable saveable in FindSaveables())
                saveables.TryAdd(saveable.SaveId.Id, saveable);

            _unusedEntries.Clear();
            foreach (SaveEntry entry in saveFile.Entries)
            {
                if (saveables.TryGetValue(entry.Id, out ISaveable saveable))
                    saveable.RestoreData(entry.Data);
                else
                    _unusedEntries.Add(entry);
            }
        }

        /// <summary> 저장 파일을 지웁니다. 인스펙터 메뉴에서 씁니다. </summary>
        [ContextMenu("Delete Save File")]
        public void DeleteSave()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);

            _unusedEntries.Clear();
        }

        // 깨진 파일을 남기면 이어하기마다 같은 실패가 반복되므로, 원인 확인용으로 옆에 옮겨 두고 새 게임처럼 시작한다
        private void MoveBrokenSave(string reason)
        {
            if (File.Exists(CorruptPath))
                File.Delete(CorruptPath);

            File.Move(SavePath, CorruptPath);
            _unusedEntries.Clear();
            Debug.LogWarning($"[SaveManager] 저장 파일 깨짐 → {CorruptPath}로 옮기고 새로 시작 (원인: {reason}): {gameObject.name}", this);
        }

        private static List<ISaveable> FindSaveables()
        {
            List<ISaveable> saveables = new();
            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>())
            {
                if (behaviour is not ISaveable saveable)
                    continue;

                // 아이디를 안 넣은 모듈은 저장 파일에서 구분할 수 없으니 빼고, 개발 중에 알 수 있게 한다
                Debug.Assert(saveable.SaveId != null, $"[SaveManager] {behaviour.GetType().Name}의 SaveId 없음: {behaviour.name}", behaviour);
                if (saveable.SaveId != null)
                    saveables.Add(saveable);
            }

            return saveables;
        }
    }
}

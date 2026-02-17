using AxGrid.Base;
using AxGrid.Model;
using UnityEngine;

namespace Lootbox
{
    /// <summary>
    /// Контроллер частиц результата — запускает систему частиц
    /// при отображении результата на слот-барабане.
    /// </summary>
    public class ResultParticleController : MonoBehaviourExtBind
    {
        [SerializeField] private ParticleSystem _particleSystem;

        [OnAwake]
        private void OnAwakeInit()
        {
            if (_particleSystem == null)
                _particleSystem = GetComponentInChildren<ParticleSystem>();
        }

        [Bind("PlayResultParticles")]
        private void OnPlayResultParticles()
        {
            if (_particleSystem != null)
                _particleSystem.Play();
        }
    }
}

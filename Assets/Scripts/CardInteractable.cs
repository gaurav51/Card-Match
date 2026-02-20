using UnityEngine;

namespace Assets.Scripts
{
    public abstract class CardInteractable : MonoBehaviour
    {
        [SerializeField]
        protected CardState cardState;

        public abstract void OnCardClicked();
    }
}

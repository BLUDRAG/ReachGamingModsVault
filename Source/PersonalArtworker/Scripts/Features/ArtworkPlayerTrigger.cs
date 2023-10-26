using UnityEngine;

namespace PersonalArtworker.Scripts.Features
{
    public class ArtworkPlayerTrigger : MonoBehaviour
    {
        private ArtworkFeatureController _controller;

        public void Init(ArtworkFeatureController controller)
        {
            _controller = controller;
        }

        private void OnTriggerEnter(Collider other)
        {
            EntityPlayerLocal player = other.GetComponent<EntityPlayerLocal>();
            
            if(player is null)
            {
                return;
            }
            
            _controller.RecalculateMultiBlockDim();
        }

        private void OnTriggerExit(Collider other)
        {
            EntityPlayerLocal player = other.GetComponent<EntityPlayerLocal>();

            if(player is null)
            {
                return;
            }
            
            _controller.ResetMultiBlockDim();
        }
    }
}
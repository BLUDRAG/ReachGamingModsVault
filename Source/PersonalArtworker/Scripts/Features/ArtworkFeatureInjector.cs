using System.Collections.Generic;
using PersonalArtworker.Scripts.Blocks;
using PersonalArtworker.Scripts.Entities;
using UnityEngine;

namespace PersonalArtworker.Scripts.Features
{
    public static class ArtworkFeatureInjector
    {
        private static readonly Dictionary<string, int> _vertexIndices = new Dictionary<string, int>()
                                                                         {
                                                                             {
                                                                                 "anchor-TopRight", 3
                                                                             },
                                                                             {
                                                                                 "anchor-TopLeft", 2
                                                                             },
                                                                             {
                                                                                 "anchor-BottomRight", 1
                                                                             },
                                                                             {
                                                                                 "anchor-BottomLeft", 0
                                                                             }
                                                                         };

        public static void InjectFeatures(ArtworkBlock block, ArtworkEntity artworkEntity, Transform root)
        {
            if(artworkEntity.Initialized) return;
            MeshFilter   artworkMesh     = root.Find("ArtworkMesh").GetComponent<MeshFilter>();
            MeshRenderer artworkRenderer = artworkMesh.GetComponent<MeshRenderer>();
            Transform    resizeRoot      = root.Find("MeshAnchors");
            Transform    triggerBox      = artworkMesh.transform.Find("TriggerBox");
            triggerBox.gameObject.SetActive(false); // TODO Currently unreliable behaviour when updating block chunk data.

            ArtworkFeatureController controller = artworkEntity.Init(block, artworkRenderer, resizeRoot.gameObject);

            ApplyInitialCanvasSize(block, artworkEntity, artworkMesh);
            InjectResizer(controller, artworkMesh, resizeRoot);
            // InjectPlayerTrigger(controller, triggerBox);
        }

        private static void InjectResizer(ArtworkFeatureController controller, MeshFilter filter, Transform anchors)
        {
            List<ArtworkResizer> anchorList = new List<ArtworkResizer>();
            Camera               camera     = Camera.main;

            foreach(Transform anchor in anchors)
            {
                ArtworkResizer scaler = anchor.gameObject.AddComponent<ArtworkResizer>();
                scaler.Controller  = controller;
                scaler.Filter      = filter;
                scaler.Camera      = camera;
                scaler.VertexIndex = _vertexIndices[anchor.name];
                anchorList.Add(scaler);
            }

            foreach(ArtworkResizer pointer in anchorList)
            {
                pointer.Links.AddRange(anchorList);
                pointer.Links.Remove(pointer);
            }
        }

        private static void ApplyInitialCanvasSize(ArtworkBlock block, ArtworkEntity artworkEntity,
                                                   MeshFilter   artworkMesh)
        {
            Vector3 loadedScale = artworkEntity.GetLoadedScale();

            if(loadedScale == Vector3.zero)
            {
                string blockName  = block.GetBlockName();
                string dimensions = blockName.Substring(blockName.Length - 3);
                int    width      = int.Parse(dimensions.Substring(0, 1));
                int    height     = int.Parse(dimensions.Substring(2, 1));
                artworkMesh.transform.localScale = new Vector3(width, height, artworkMesh.transform.localScale.z);
            }
        }
        
        private static void InjectPlayerTrigger(ArtworkFeatureController controller, Transform triggerBox)
        {
            ArtworkPlayerTrigger trigger = triggerBox.gameObject.AddComponent<ArtworkPlayerTrigger>();
            trigger.Init(controller);
        }
    }
}
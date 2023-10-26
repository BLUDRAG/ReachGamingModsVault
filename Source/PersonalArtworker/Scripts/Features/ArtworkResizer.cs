using System.Collections.Generic;
using UnityEngine;

namespace PersonalArtworker.Scripts.Features
{
	[DefaultExecutionOrder(-100)]
    public class ArtworkResizer : MonoBehaviour
	{
		public List<ArtworkResizer>     Links = new List<ArtworkResizer>();
		public Camera                   Camera;
		public ArtworkFeatureController Controller;

		public MeshFilter Filter;
		public Transform  Transform;

		/// <summary>
		/// Top Left     : 21
		/// Top Right    : 18
		/// Bottom Left  : 12
		/// Bottom Right : 15
		/// </summary>
		public int VertexIndex;

		private Transform    _filterTransform;
		private Transform    _cameraTransform;
		private Material     _material;
		private MeshRenderer _meshRenderer;
		private Vector3      _originalPosition;
		private Vector3      _originalLocalPosition;
		private Vector3      _originalScale;
		private Vector3      _originalDistance;
		private Vector3[]    _vertices;
		private Ray          _ray;
		private Plane        _plane;
		private float        _aspectRatio;
		private bool         _inputValid;

		private const float MAX_RAY_DISTANCE = 5f;
		
		private void Start()
		{
			Transform        = transform;
			_filterTransform = Filter.transform;
			_cameraTransform = Camera.transform;
			_material        = Filter.GetComponent<MeshRenderer>().material;
			_meshRenderer    = GetComponent<MeshRenderer>();

			_ray      = new Ray(_cameraTransform.position, _cameraTransform.forward);
			_plane    = new Plane(_filterTransform.forward, Transform.position);
			_vertices = Filter.sharedMesh.vertices;

			UpdateVertexPosition();
		}
		
		private void Update()
		{
			if(ArtworkFeatureController.IsLegacyInputActive)
			{
				return;
			}

			if(Input.GetMouseButtonDown(0))
			{
				ValidateInput();

				if(!_inputValid)
				{
					return;
				}

				HandleMouseDown();
			}
			else if(_inputValid && Input.GetMouseButton(0))
			{
				HandleMouseDrag();
			}
			else if(_inputValid && Input.GetMouseButtonUp(0))
			{
				HandleMouseUp();
				_inputValid = false;
			}
		}

		private void OnMouseDown()
		{
			if(!ArtworkFeatureController.IsLegacyInputActive)
			{
				return;
			}
			HandleMouseDown();
		}

		private void OnMouseDrag()
		{
			if(!ArtworkFeatureController.IsLegacyInputActive)
			{
				return;
			}
			
			HandleMouseDrag();
		}

		private void OnMouseUp()
		{
			if(!ArtworkFeatureController.IsLegacyInputActive)
			{
				return;
			}
			
			HandleMouseUp();
		}

		private void HandleMouseDown()
		{
			GameInteractions.GameInteractions.TogglePrimaryAction(false);
			_originalPosition      = _filterTransform.position;
			_originalLocalPosition = _filterTransform.localPosition;
			_originalScale         = _filterTransform.localScale;
			_originalDistance      = _originalPosition - GetTouchPoint();

			if(Controller.IsAspectLocked())
			{
				_aspectRatio = (float)_material.mainTexture.width / _material.mainTexture.height;
			}
		}

		private void HandleMouseDrag()
		{
			Vector3 transformForward = _filterTransform.forward;
			bool    factorForward    = transformForward == Vector3.left || transformForward == Vector3.right;

			Vector3 distance = _originalPosition - GetTouchPoint();
			(float forward, float y) = Factor(distance, _originalDistance, factorForward);

			if(Controller.IsAspectLocked())
			{
				forward = y * _aspectRatio;
			}

			Vector3 adjustedDistance = _originalDistance;
			adjustedDistance.x *= transformForward == Vector3.forward ? -forward : forward;
			adjustedDistance.y *= y;

			Vector3 position = _originalLocalPosition - adjustedDistance;
			position.z = _originalLocalPosition.z;

			Vector3 adjustedScale = _originalScale;
			adjustedScale.x *= forward;
			adjustedScale.y *= y;

			Vector3 scale = _originalScale + adjustedScale;
			scale.z = _originalScale.z;

			_filterTransform.localPosition = position;
			_filterTransform.localScale    = scale;

			UpdateVertexPosition();

			foreach(ArtworkResizer link in Links)
			{
				link.UpdateVertexPosition();
			}
		}

		private void HandleMouseUp()
		{
			GameInteractions.GameInteractions.TogglePrimaryAction(true);
			Controller.SaveData();
		}

		private void ValidateInput()
		{
			_ray.origin    = _cameraTransform.position;
			_ray.direction = _cameraTransform.forward;

			_inputValid = _plane.Raycast(_ray, out float distance);
			Vector3 touchPoint = _ray.GetPoint(distance);
			_inputValid = _meshRenderer.bounds.Contains(touchPoint);
		}

		public void UpdateVertexPosition()
		{
			Transform.position = _filterTransform.TransformPoint(_vertices[VertexIndex]);
		}

		private Vector3 GetTouchPoint()
		{
			_ray.origin    = _cameraTransform.position;
			_ray.direction = _cameraTransform.forward;
			
			_plane.Raycast(_ray, out float distance);

			return _ray.GetPoint(Mathf.Clamp(distance, -MAX_RAY_DISTANCE, MAX_RAY_DISTANCE));
		}

		private static (float forward, float y) Factor(Vector3 numerator, Vector3 denominator, bool factorForward)
		{
			float y = numerator.y / denominator.y - 1f;
			float forward = (factorForward ? numerator.z : numerator.x) / (factorForward ? denominator.z : denominator.x) - 1f;
			
			return (factorForward ? forward : forward / 2f, y / 2f);
		}
	}
}
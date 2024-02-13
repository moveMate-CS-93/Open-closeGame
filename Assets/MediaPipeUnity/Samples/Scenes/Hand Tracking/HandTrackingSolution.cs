// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mediapipe.Unity.Sample.HandTracking
{
    public class HandTrackingSolution : ImageSourceSolution<HandTrackingGraph>
    {
        [SerializeField] private DetectionListAnnotationController _palmDetectionsAnnotationController;
        [SerializeField] private NormalizedRectListAnnotationController _handRectsFromPalmDetectionsAnnotationController;
        [SerializeField] private MultiHandLandmarkListAnnotationController _handLandmarksAnnotationController;
        [SerializeField] private NormalizedRectListAnnotationController _handRectsFromLandmarksAnnotationController;

        private bool fingersClosed = false;
        private bool jumpRequested = false;

        public HandTrackingGraph.ModelComplexity modelComplexity
        {
            get => graphRunner.modelComplexity;
            set => graphRunner.modelComplexity = value;
        }

        public int maxNumHands
        {
            get => graphRunner.maxNumHands;
            set => graphRunner.maxNumHands = value;
        }

        public float minDetectionConfidence
        {
            get => graphRunner.minDetectionConfidence;
            set => graphRunner.minDetectionConfidence = value;
        }

        public float minTrackingConfidence
        {
            get => graphRunner.minTrackingConfidence;
            set => graphRunner.minTrackingConfidence = value;
        }

        protected override void OnStartRun()
        {
            graphRunner.OnPalmDetectectionsOutput += OnPalmDetectionsOutput;
            graphRunner.OnHandRectsFromPalmDetectionsOutput += OnHandRectsFromPalmDetectionsOutput;
            graphRunner.OnHandLandmarksOutput += OnHandLandmarksOutput;
            // TODO: render HandWorldLandmarks annotations
            graphRunner.OnHandRectsFromLandmarksOutput += OnHandRectsFromLandmarksOutput;
            graphRunner.OnHandednessOutput += OnHandednessOutput;

            var imageSource = ImageSourceProvider.ImageSource;
            SetupAnnotationController(_palmDetectionsAnnotationController, imageSource, true);
            SetupAnnotationController(_handRectsFromPalmDetectionsAnnotationController, imageSource, true);
            SetupAnnotationController(_handLandmarksAnnotationController, imageSource, true);
            SetupAnnotationController(_handRectsFromLandmarksAnnotationController, imageSource, true);
        }
        protected override void AddTextureFrameToInputStream(TextureFrame textureFrame)
        {
            graphRunner.AddTextureFrameToInputStream(textureFrame);
        }

        protected override IEnumerator WaitForNextValue()
        {
            var task = graphRunner.WaitNext();
            yield return new WaitUntil(() => task.IsCompleted);

            var result = task.Result;
            _palmDetectionsAnnotationController.DrawNow(result.palmDetections);
            _handRectsFromPalmDetectionsAnnotationController.DrawNow(result.handRectsFromPalmDetections);
            _handLandmarksAnnotationController.DrawNow(result.handLandmarks, result.handedness);

            // Check if fingers are closed and request jump
            if (fingersClosed && !jumpRequested)
            {
              jumpRequested = true;
              GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

              if (playerObject != null) {
                  Debug.Log("Player GameObject found.");
                  dino dinoComponent = playerObject.GetComponent<dino>();

                  if (dinoComponent != null) {
                      dinoComponent.RequestJump();
                  } else {
                      Debug.LogError("dino component not found on the Player GameObject.");
                  }
              } else {
                  Debug.LogError("Player GameObject not found.");
              }

            }
            else if (!fingersClosed && jumpRequested)
            {
                jumpRequested = false;
            }

            _handRectsFromLandmarksAnnotationController.DrawNow(result.handRectsFromLandmarks);
        }

        private void OnPalmDetectionsOutput(object stream, OutputStream<List<Detection>>.OutputEventArgs eventArgs)
        {
            var packet = eventArgs.packet;
            var value = packet == null ? default : packet.Get(Detection.Parser);
            _palmDetectionsAnnotationController.DrawLater(value);
        }

        private void OnHandRectsFromPalmDetectionsOutput(object stream, OutputStream<List<NormalizedRect>>.OutputEventArgs eventArgs)
        {
            var packet = eventArgs.packet;
            var value = packet == null ? default : packet.Get(NormalizedRect.Parser);
            _handRectsFromPalmDetectionsAnnotationController.DrawLater(value);
        }

        private void OnHandLandmarksOutput(object stream, OutputStream<List<NormalizedLandmarkList>>.OutputEventArgs eventArgs)
        {
            var packet = eventArgs.packet;
            var value = packet == null ? default : packet.Get(NormalizedLandmarkList.Parser);

            if (value != null && value.Count > 0 && value[0].Landmark.Count >= 21)
            {
                // Check the state of all fingers together
                bool allFingersOpen = true;

                for (int i = 1; i <= 20; i += 4) // Start from THUMB_CMC (1) and check every 4th joint
                {
                    Vector3 fingerTipPosition = new Vector3(value[0].Landmark[i + 3].X, value[0].Landmark[i + 3].Y, value[0].Landmark[i + 3].Z);
                    Vector3 knucklePosition = new Vector3(value[0].Landmark[i].X, value[0].Landmark[i].Y, value[0].Landmark[i].Z);

                    float fingerDistance = Vector3.Distance(fingerTipPosition, knucklePosition);

                    // Log the finger distance for debugging
                    Debug.Log($"Finger {i / 4 + 1} Distance: {fingerDistance}");

                    // If any finger is closed, set allFingersOpen to false
                    if (fingerDistance <= 0.05f)
                    {
                        allFingersOpen = false;
                    }
                }

                if (allFingersOpen)
                {
                    Debug.Log("Hand is open");
                }
                else
                {
                    // Reset jump when fingers are closed
                    fingersClosed = true;
                    jumpRequested = false;

                    Debug.Log("Hand is closed, dino jumping");
                }
            }

            _handLandmarksAnnotationController.DrawLater(value);
        }

        private void OnHandRectsFromLandmarksOutput(object stream, OutputStream<List<NormalizedRect>>.OutputEventArgs eventArgs)
        {
            var packet = eventArgs.packet;
            var value = packet == null ? default : packet.Get(NormalizedRect.Parser);
            _handRectsFromLandmarksAnnotationController.DrawLater(value);
        }

        private void OnHandednessOutput(object stream, OutputStream<List<ClassificationList>>.OutputEventArgs eventArgs)
        {
            var packet = eventArgs.packet;
            var value = packet == null ? default : packet.Get(ClassificationList.Parser);
            _handLandmarksAnnotationController.DrawLater(value);
        }
    }
}

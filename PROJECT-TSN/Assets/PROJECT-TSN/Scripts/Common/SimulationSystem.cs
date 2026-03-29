using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TST
{
    public class SimulationSystem : MonoBehaviour
    {
        public static SimulationSystem Instance { get; private set; }

        public Collider[] colliders;
        public int simulationStep = 100;

        private UnityEngine.SceneManagement.Scene simulationScene;
        private PhysicsScene physicsScene;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            simulationScene = SceneManager.CreateScene("Simulation Scene", new CreateSceneParameters(LocalPhysicsMode.Physics3D));

            for (int i = 0; i < colliders.Length; i++)
            {
                var ghostObject = Instantiate(colliders[i]);
                if (ghostObject.TryGetComponent(out Renderer renderer))
                {
                    renderer.enabled = false;
                }
                SceneManager.MoveGameObjectToScene(ghostObject.gameObject, simulationScene);
            }

            physicsScene = simulationScene.GetPhysicsScene();
        }

        public List<Vector3> Simulate(Rigidbody throwObject, Vector3 startPosition, Vector3 force, ForceMode mode)
        {
            Rigidbody ghostThrowObject = Instantiate(throwObject);
            ghostThrowObject.transform.position = startPosition;
            if (ghostThrowObject.TryGetComponent(out Renderer renderer))
            {
                renderer.enabled = false;
            }

            SceneManager.MoveGameObjectToScene(ghostThrowObject.gameObject, simulationScene);
            ghostThrowObject.gameObject.SetActive(true);
            ghostThrowObject.isKinematic = false;
            ghostThrowObject.AddForce(force, mode);

            List<Vector3> simulationPositions = new List<Vector3>();
            for (int i = 0; i < simulationStep; i++)
            {
                simulationPositions.Add(ghostThrowObject.position);
                physicsScene.Simulate(Time.fixedDeltaTime);
            }

            Destroy(ghostThrowObject.gameObject);

            return simulationPositions;
        }
    }
}

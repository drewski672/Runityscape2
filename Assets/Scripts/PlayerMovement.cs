using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerMovement : NetworkBehaviour
{
    private NavMeshAgent _navMeshAgent;
    [SerializeField] public Camera playerCamera;
    [SerializeField] public CinemachineFreeLook cinemachineFreeLookCam;
    private Vector3 _destination;

    public override void OnNetworkSpawn()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        if (_navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent is null!");
        }
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            Debug.LogError("playerCamera in Awake() is null!");
        }

        cinemachineFreeLookCam = GetComponentInChildren<CinemachineFreeLook>();
        if (cinemachineFreeLookCam == null)
        {
            Debug.LogError("cinemachineFreeLookCam in Awake() is null!");
        }

        if (IsOwner)
        {
            if (playerCamera == null)
            {
                Debug.LogError("playerCamera in OnNetworkSpawn() is null!");
                return;
            }

            cinemachineFreeLookCam.Priority = 1;
        }
        else
        {
            cinemachineFreeLookCam.Priority = 0;
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Left Mouse button clicked");
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 1); //add this line
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.Log("Attempting to move to: " + hit.point);
                RequestMoveServerRpc(hit.point);
            }
        }

    }

    // The client calls this function to request a move
    [ServerRpc]
    public void RequestMoveServerRpc(Vector3 point)
    {
        Move(point);
        MoveClientRpc(point);
    }

    // Only the server can actually move the player
    [ClientRpc]
    public void MoveClientRpc(Vector3 point)
    {
        Move(point);
    }

    // The client calls this function to request a move to a specific destination
    //[ServerRpc]
    //public void RequestMoveToServerRpc(Vector3 destination)
    //{
    //    MoveToClientRpc(destination);
    //}

    // Only the server can actually move the player to a specific destination
    //[ClientRpc]
    //public void MoveToClientRpc(Vector3 destination)
    //{
    //    MoveTo(destination);
    //}

    private void Move(Vector3 point)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(point, out hit, 1.0f, NavMesh.AllAreas))
        {
            _navMeshAgent.SetDestination(RoundToNearestCellCenter(hit.position));
        }
    }

    //private void MoveTo(Vector3 destination)
    //{
    //    _navMeshAgent.SetDestination(destination);
    //}

    private Vector3 RoundToNearestCellCenter(Vector3 rawWorldPos)
    {
        int x = Mathf.RoundToInt(rawWorldPos.x);
        int y = Mathf.RoundToInt(rawWorldPos.y);
        int z = Mathf.RoundToInt(rawWorldPos.z);

        return new Vector3(x, y, z);
    }
}

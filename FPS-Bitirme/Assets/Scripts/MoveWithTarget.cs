using UnityEngine;

public class MoveWithTarget : MonoBehaviour
{
    #region COMPONENTS
    [SerializeField]
    private GameObject target;
    #endregion
    
    void Update()
    {
        if (target == null)
        {
            return;
        }

        transform.position = Vector3.Lerp(transform.position, target.transform.position, Time.deltaTime);
    }
}

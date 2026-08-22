using UnityEngine;

namespace Invector.Utils
{
    public class vComment : MonoBehaviour
    {
        [SerializeField] protected string header = "COMMENT";
        [Multiline]
        [SerializeField] protected string comment;

        [SerializeField] protected bool inEdit;
    }
}
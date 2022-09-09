using UnityEngine;
using System.Collections.Generic;

namespace Invector.vShooter
{
    [vClassHeader("Decal Manager", openClose = false)]
    public class vDecalManager : vMonoBehaviour
    {
        public LayerMask layermask;
      
        public List<DecalObject> decalObjects;

        public virtual void CreateDecal(RaycastHit hitInfo)
        {
            CreateDecal(hitInfo.collider.gameObject, hitInfo.point, hitInfo.normal);
        }

        public virtual void CreateDecal(GameObject target, Vector3 position, Vector3 normal)
        {
            if (layermask == (layermask | (1 << target.layer)))
            {
                Renderer rend = target.GetComponent<Renderer>();
                DecalObject decalObj = GetDecal(target.tag, rend != null ? rend?.material.name : "");
                if (decalObj != null)
                {
                    RaycastHit hit;
                    if (Physics.SphereCast(new Ray(position + (normal * 0.1f), -normal), 0.0001f, out hit, 1f, layermask))
                    {

                        if (hit.collider.gameObject == target)
                        {
                            var rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
                            decalObj.CreateEffect(hit.point, hit.normal, rotation,gameObject, target);
                        }
                    }
                }
            }
        }

        protected virtual DecalObject GetDecal(string tag, string materialName)
        {
            if (!string.IsNullOrEmpty(materialName))
            {
                foreach (DecalObject obj in decalObjects)
                {
                    foreach(string m in obj.materialNames)
                    {
                        if (materialName.ToUpper().IndexOf(m.ToUpper()) >= 0)
                        {
                            return obj;
                        }
                    }
                }
            }

            return decalObjects.Find(d=>d.tag.Equals(tag));
        }

        [System.Serializable]
        public class DecalObject
        {
            public bool byMaterialName;

            public string tag;
            public List<string> materialNames = new List<string>();

            [SerializeField] protected vImpactEffectBase impactEffect;
            [SerializeField] protected List<vImpactEffectBase> additionalEffects;
            public void CreateEffect(Vector3 position, Vector3 normal, Quaternion rotation, GameObject impactSender, GameObject impactReceiver)
            {
                impactEffect.DoImpactEffect(position, normal, rotation,impactSender, impactReceiver);
                for(int i=0;i<additionalEffects.Count;i++)
                {
                    additionalEffects[i].DoImpactEffect(position, normal, rotation, impactSender, impactReceiver);
                }
             
            }
        }
    }
}
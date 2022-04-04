using UnityEngine;
using System;
using NaughtyAttributes;
using Invector.vMelee;
using System.Collections.Generic;

[Serializable]
public enum SurfaceType
{
    Custom,
    BFX_Blood,
    Bullet_Impact_FX
}

public enum HitType
{
    All,
    Melee,
    Shooter
}

public enum Bullet_Impact_Names
{
    BrickWall,
    Concrete,
    ElectricDevices,
    Fabric,
    Flash,
    Glass_BulletProff,
    Glass_Common,
    Glass_Windshield,
    Ground,
    Metal_Tinplate,
    MetalSolid,
    Rocks,
    Rubber_Plastic,
    Sand,
    Sheetrock,
    SndBags,
    Snow,
    Water,
    Wood
}

[Serializable]
public class Bullet_Impact_Materials
{
    public bool usePhysicsMaterial;

    [Tag]
    [HideIf("usePhysicsMaterial")]
    public string tag;

    [AllowNesting]
    public Bullet_Impact_Names effect;

    public HitType hitType;
}


[Serializable]
public class BFX_BloodSurface
{
    [Tag]
    public string tag;
    public HitType hitType;
    public List<BFX_BloodSettings> bloodEffects;
}

[Serializable]
public class CustomEffect
{
    [Tag]
    public string tag;
    public HitType hitType;
    public GameObject effect;
    public float lifeTime;
}

[Serializable]
public class SurfaceHitEffect
{
    [SerializeField]
    public List<CustomEffect> customEffects = new List<CustomEffect>();
    [SerializeField]
    public List<Bullet_Impact_Materials> bulletImpactMaterials = new List<Bullet_Impact_Materials>();
    [SerializeField]
    public List<BFX_BloodSurface> BFXBloodSurfaces = new List<BFX_BloodSurface>();
}


[CreateAssetMenu(fileName = "ObjectHits", menuName = "ScriptableObjects/SurafceHitEffects", order = 1)]
public class SurfaceHitEffects : ScriptableObject
{
    [InfoBox("To create impact effects on your tags just define the Tag and Hit Type (if required).  When checking against colliders, the Tag & Hit Type must match.  If Hit Type is set to Default, then only the Tag will be checked. Effect Surfaces are checked in the following order\n1. Custom Effects\n2. Bullet Impact Effects\n3.KriptoFX Volumetric Blood Effects.\nBullet Impact Effects uses its own Pool Manager, the others do not.\nMake sure you utilize Life Time & Initial Pool Size unless your prefab has its own Destroy system.", EInfoBoxType.Normal)]
    public SurfaceHitEffect surfaces;

    Queue<BFX_BloodSettings> activeBlood = new Queue<BFX_BloodSettings>();

    public void TriggerEffect(GameObject player, vHitInfo hitInfo, HitType hitType = HitType.All)
    {
        int idx;

        idx = surfaces.customEffects.FindIndex(e => 
            hitInfo.targetCollider.CompareTag(e.tag) && (e.hitType == hitType || e.hitType == HitType.All));
        if (idx >= 0)
        {
            CustomEffect hitEffect = surfaces.customEffects[idx];

            float angle = Mathf.Atan2(hitInfo.hitNormal.x, hitInfo.hitNormal.z) * Mathf.Rad2Deg + 180;
            var hit = Instantiate(hitEffect.effect, hitInfo.hitPoint, Quaternion.Euler(0, angle + 90, 0));

            // For those effects that don't handle their own destruction
            if (hitEffect.lifeTime > 0)
                Destroy(hit, hitEffect.lifeTime);
            return;
        }

        idx = surfaces.bulletImpactMaterials.FindIndex(e => e.usePhysicsMaterial ?
            hitInfo.targetCollider.material.name.ToLower().Contains(e.effect.ToString().ToLower()) && (e. hitType == hitType || e.hitType == HitType.All) : 
            hitInfo.targetCollider.CompareTag(e.tag) && (e.hitType == hitType || e.hitType == HitType.All));

        if (idx >= 0)
        {
            string name = surfaces.bulletImpactMaterials[idx].effect.ToString();
            WPN_Decal_Manager.Instance.SpawnBulletHitEffects(hitInfo.hitPoint, hitInfo.hitNormal, name, hitInfo.targetCollider.gameObject);
            return;
        }

        idx = surfaces.BFXBloodSurfaces.FindIndex(e => hitInfo.targetCollider.CompareTag(e.tag) && (e.hitType == hitType || e.hitType == HitType.All));
        if (idx >= 0)
        {
            int rndEffect = UnityEngine.Random.Range(0, surfaces.BFXBloodSurfaces[idx].bloodEffects.Count);

            BFX_BloodSettings effect = surfaces.BFXBloodSurfaces[idx].bloodEffects[rndEffect];
            effect.GroundHeight = player.transform.position.y;//hitInfo.attackObject.transform.position.y;

            float angle = Mathf.Atan2(hitInfo.hitNormal.x, hitInfo.hitNormal.z) * Mathf.Rad2Deg + 180;
            var blood = Instantiate(effect, hitInfo.hitPoint, Quaternion.Euler(0, angle + 90, 0));
            var shader = blood.GetComponentInChildren<BFX_ShaderProperies>();
            shader.OnAnimationFinished += DestroyBlood;
            activeBlood.Enqueue(blood);
        }
    }

    void DestroyBlood()
    {
        Destroy(activeBlood.Dequeue());
    }
}

using System.Collections.Generic;
using KarlsonMultiplayer.Shared;
using RiptideNetworking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarlsonMultiplayer.Multiplayer.Client
{
    public class ClientPlayer
    {

        public ushort id;
        public string username;

        public GameObject playerObject;

        public string currentLoadedScene;

        public string currentWeapon;
        public GameObject weaponObject;

        public Vector3 wantedPosition = Vector3.zero;
        public float timeSyncing;

        public void Move(Vector3 newPos, Quaternion newRot)
        {
            if (!SceneManager.GetActiveScene().name.Equals(currentLoadedScene))
            {
                newPos.y += 9999999;
            }
            
            playerObject.transform.position = wantedPosition;
            
            wantedPosition = newPos;
            playerObject.transform.rotation = newRot;
            timeSyncing = 0;
        }

        public void LerpPosition()
        {
            timeSyncing += Time.deltaTime / Time.fixedDeltaTime;
            timeSyncing = Mathf.Clamp(timeSyncing, 0, 1);
            
            Vector3 newPos = Vector3.Lerp(playerObject.transform.position, wantedPosition, timeSyncing);
            
            playerObject.transform.position = newPos;
        }

        public void RemovePlayer()
        {
            Main.instance.DestroyObject(playerObject);
            ClientPlayerManager.List.Remove(id);
        }

        public void ShootProjectile(Vector3 dir)
        {
            weaponObject.GetComponent<RangedWeapon>().SpawnProjectile(dir);
        }

        public void SpawnWeapon()
        {
            weaponObject = Main.instance.SpawnObject(GameObject.Find(currentWeapon));
            Main.instance.DestroyObject(weaponObject.GetComponent<Rigidbody>());
            
            weaponObject.GetComponent<RangedWeapon>().PickupWeapon(false);
        }

        public void Crouch(bool crouchState)
        {
            
            playerObject.transform.localScale = crouchState ? new Vector3(1, 0.5f, 1) : new Vector3(1, 1.5f, 1);
        }
    }
}
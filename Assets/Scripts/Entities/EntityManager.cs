using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    static EntityManager instance;
    public static EntityManager Instance
    {
        get
        {
            return instance;
        }
    }
    int amountOfEnemiesInRoom;
    bool allEnemiesOfRoomDefeated()
    {
        return amountOfEnemiesInRoom == 0;
    }

    [System.Serializable]public class ProjectileEntry
    {
        public int ID;
        public Transform transform;
        public ProjectileController projectileController;

        public ProjectileEntry(int ID_in, Transform transform_in, ProjectileController proj_in)
        {
            ID = ID_in;
            transform = transform_in;
            projectileController = proj_in;
        }
    }
    List<ProjectileEntry> projectiles = new List<ProjectileEntry>();

    private void Awake() 
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
            instance = this;
        }
    }

    public void Add(ref ProjectileController proj)
    {
        projectiles.Add(new ProjectileEntry(projectiles.Count, proj.transform, proj));
        proj.ID = projectiles.Count - 1;
    }
    public void Remove(int ID)
    {
        for(int i = ID + 1; i < projectiles.Count; i++)
        {
            projectiles[i].ID--;
            projectiles[i].projectileController.ID--;
        }
        projectiles.RemoveAt(ID);
    }

    public void CheckProjectileGrassCollision(Room room)
    {
        for(int i = 0; i < projectiles.Count; i++)
        {
            Vector3 pos = new Vector3(projectiles[i].transform.position.x, -projectiles[i].transform.position.y) + new Vector3(-room.transform.position.x, room.transform.position.y) + new Vector3(10, 9, 0);
            //The positions y has to be adjusted so that it starts at 0 in the top corner and goes up when going down
            room.grass.CheckCollision(pos, projectiles[i].transform.position);
        }
    }
}

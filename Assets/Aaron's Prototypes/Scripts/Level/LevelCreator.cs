using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelEditor
{
    public class LevelCreator : MonoBehaviour
    {
        LevelManager manager;
        GridBase gridBase;
        InterfaceManager ui;

        //place obj variables
        bool hasObj;
        GameObject objToPlace;
        GameObject cloneObj;
        Level_Object objProperties;
        Vector3 mousePosition;
        Vector3 worldPosition;
        bool deleteObj;

        //paint tile variable
        bool hasMaterial;
        bool paintTile;
        public Material matToPlace;
        Node previousNode;
        Material prevMaterial;
        Quaternion targetRot;
        Quaternion prevRotation;

        //place stack objs variables
        bool placeStackObj;
        GameObject stackObjToPlace;
        GameObject stackCloneObj;
        Level_Object stackObjProperties;
        bool deleteStackObj;

        //Wall creator variables
        bool createWall;
        public GameObject wallPrefab;
        Node startNode_Wall;
        Node endNodeWall;
       // public Material[] wallPlacementMat;
        bool deleteWall;

        void Start()
        {
            gridBase = GridBase.GetInstance();
            manager = LevelManager.GetInstance();
            ui = InterfaceManager.GetInstance();

            PaintAll();
        }

        void Update()
        {
            PlaceObject();
            PaintTile();
            DeleteObjs();
            PlaceStackedObj();
            CreateWall();
            DeleteStackedObjs();
            DeleteWallsActual();
        }

        void UpdateMousePosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(Physics.Raycast(ray,out hit, Mathf.Infinity))
            {
                mousePosition = hit.point;
            }
        }

        #region Place Objects
        void PlaceObject()
        {
            if (hasObj)
            {
                UpdateMousePosition();

                Node curNode = gridBase.NodeFromWorldPosition(mousePosition);

                worldPosition = curNode.vis.transform.position;

                if (cloneObj == null)
                {
                    cloneObj = Instantiate(objToPlace, worldPosition, Quaternion.identity) as GameObject;
                    objProperties = cloneObj.GetComponent<Level_Object>();
                }
                else
                {
                    cloneObj.transform.position = worldPosition;
                    if (Input.GetMouseButtonDown(0) && !ui.mouseOverUIElement)
                    {
                        if (curNode.placedObj != null)
                        {
                            manager.inSceneGameObjects.Remove(curNode.placedObj.gameObject);
                            Destroy(curNode.placedObj.gameObject);
                            curNode.placedObj = null;
                        }

                        GameObject actualObjPlaced = Instantiate(objToPlace, worldPosition, cloneObj.transform.rotation) as GameObject;
                        Level_Object placedObjProperties = actualObjPlaced.GetComponent<Level_Object>();

                        placedObjProperties.gridPosX = curNode.nodePosX;
                        placedObjProperties.gridPosZ = curNode.nodePosZ;
                        curNode.placedObj = placedObjProperties;
                        manager.inSceneGameObjects.Add(actualObjPlaced);
                    }

                    if (Input.GetMouseButtonDown(1))
                    {
                        objProperties.ChangeRotation();
                    }
                }
            }
            else
            {
                if (cloneObj != null)
                {
                    Destroy(cloneObj);
                }
            }
        }

        public void PassGameObjectToPlace(string objId)
        {
            if (cloneObj != null)
            {
                Destroy(cloneObj);
            }

            CloseAll();
            hasObj = true;
            cloneObj = null;
            objToPlace = ResourcesManager.GetInstance().GetObjBase(objId).objPrefab;
        }
        void DeleteObjs()
        {
            if (deleteObj)
            {
                UpdateMousePosition();

                Node curNode = gridBase.NodeFromWorldPosition(mousePosition);

                if (Input.GetMouseButtonDown(0) && !ui.mouseOverUIElement)
                {
                    if (curNode.placedObj != null)
                    {
                        if (manager.inSceneGameObjects.Contains(curNode.placedObj.gameObject))
                        {
                            manager.inSceneGameObjects.Remove(curNode.placedObj.gameObject);
                            Destroy(curNode.placedObj.gameObject);
                        }
                        curNode.placedObj = null;
                    }
                }
            }
        }    
        public void DeleteObj()
        {
            CloseAll();
            deleteObj = true;
        }
        #endregion

        #region Stack Objects
       
        public void PassStackedObjectToPlace(string objId)
        {
            if (stackCloneObj != null)
            {
                Destroy(stackCloneObj);
            }

            CloseAll();
            placeStackObj = true;
            stackCloneObj = null;
            stackObjToPlace = ResourcesManager.GetInstance().GetStackObjBase(objId).objPrefab;
        }

        void PlaceStackedObj()
        {
            if (placeStackObj)
            {
                UpdateMousePosition();

                Node curNode = gridBase.NodeFromWorldPosition(mousePosition);

                worldPosition = curNode.vis.transform.position;

                if (stackCloneObj == null)
                {
                    stackCloneObj = Instantiate(stackObjToPlace, worldPosition, Quaternion.identity) as GameObject;
                    stackObjProperties = stackCloneObj.GetComponent<Level_Object>();
                }
                else
                {
                    stackCloneObj.transform.position = worldPosition;

                    if (Input.GetMouseButtonDown(0) && !ui.mouseOverUIElement)
                    {                       
                        GameObject actualObjPlaced = Instantiate(stackObjToPlace, worldPosition, cloneObj.transform.rotation) as GameObject;
                        Level_Object placedObjProperties = actualObjPlaced.GetComponent<Level_Object>();

                        placedObjProperties.gridPosX = curNode.nodePosX;
                        placedObjProperties.gridPosZ = curNode.nodePosZ;
                        curNode.stackedObjs.Add(placedObjProperties);
                        manager.inSceneStackObjects.Add(actualObjPlaced);
                    }

                    if (Input.GetMouseButtonDown(1))
                    {
                        stackObjProperties.ChangeRotation();
                    }
                }
            }
            else
            {
                if (stackCloneObj != null)
                {
                    Destroy(stackCloneObj);
                }
            }
        }

        void DeleteStackedObjs()
        {
            if (deleteStackObj)
            {
                UpdateMousePosition();

                Node curNode = gridBase.NodeFromWorldPosition(mousePosition);

                if (Input.GetMouseButtonDown(0) && !ui.mouseOverUIElement)
                {
                    if (curNode.stackedObjs.Count > 0)
                    {
                        for (int i = 0; i < curNode.stackedObjs.Count; i++)
                        {
                            if (manager.inSceneStackObjects.Contains(curNode.stackedObjs[i].gameObject))
                            {
                                manager.inSceneStackObjects.Remove(curNode.stackedObjs[i].gameObject);
                                Destroy(curNode.stackedObjs[i].gameObject);
                            }
                        }
                        curNode.stackedObjs.Clear();                   
                    }
                }
            }
        }
        public void DeleteStackedObj()
        {
            CloseAll();
            deleteStackObj = true;
        }
        #endregion

        #region Tile Painting

        void PaintTile()
        {
            if(hasMaterial)
            {
                UpdateMousePosition();
                Node curNode = gridBase.NodeFromWorldPosition(mousePosition);

                if(previousNode == null)
                {
                    previousNode = curNode;
                    prevMaterial = previousNode.tileRenderer.material;
                    prevRotation = previousNode.vis.transform.rotation;
                }
                else
                {
                    if (previousNode != curNode)
                    {
                        if (paintTile)
                        {
                            int matId = ResourcesManager.GetInstance().GetMaterialID(matToPlace);
                            curNode.vis.GetComponent<NodeObject>().textureid = matId;
                            paintTile = false;
                        }
                        else
                        {
                            previousNode.tileRenderer.material = prevMaterial;
                            previousNode.vis.transform.rotation = prevRotation;
                        }

                        previousNode = curNode;
                        prevMaterial = curNode.tileRenderer.material;
                        prevRotation = curNode.vis.transform.rotation;
                    }
                }

                curNode.tileRenderer.material = matToPlace;
                curNode.vis.transform.localRotation = targetRot;

                if(Input.GetMouseButton(0) && !ui.mouseOverUIElement)
                {
                    paintTile = true;
                }

                if (Input.GetMouseButtonUp(1))
                {
                    Vector3 eulerAngles = curNode.vis.transform.eulerAngles;
                    eulerAngles += new Vector3(0, 90, 0);
                    targetRot = Quaternion.Euler(eulerAngles);
                }
            }
        }

        public void PassMaterialToPaint(int matId)
        {
            deleteObj = false;
            placeStackObj = false;
            hasObj = false;
            matToPlace = ResourcesManager.GetInstance().GetMaterial(matId);
            hasMaterial = true;
        }

        public void PaintAll()
        {
            for (int x = 0; x <gridBase.sizeX;x++)
            {
                for (int z = 0; z < gridBase.sizeZ; z++)
                {
                    gridBase.grid[x, z].tileRenderer.material = matToPlace;
                    int matId = ResourcesManager.GetInstance().GetMaterialID(matToPlace);
                    gridBase.grid[x, z].vis.GetComponent<NodeObject>().textureid = matId;
                }
            }
            previousNode = null;
        }

        #endregion

        #region Wall Creator

        public void OpenWallCreation()
        {
            CloseAll();
            createWall = true;
        }

        void CreateWall()
        {
            if (createWall)
            {
                UpdateMousePosition();
                Node curNode = gridBase.NodeFromWorldPosition(mousePosition);
                worldPosition = curNode.vis.transform.position;

                if (startNode_Wall == null)
                {
                    if (Input.GetMouseButton(0) && !ui.mouseOverUIElement)
                    {
                        startNode_Wall = curNode;
                    }
                }
                else
                {
                    if(Input.GetMouseButtonUp(0) && !ui.mouseOverUIElement)
                    {
                        endNodeWall = curNode;
                    }
                }

                if (startNode_Wall != null && endNodeWall != null)
                {
                    int difX = endNodeWall.nodePosX - startNode_Wall.nodePosX;
                    int difZ = endNodeWall.nodePosZ - startNode_Wall.nodePosZ;

                    CreateWallInNode(startNode_Wall.nodePosX, startNode_Wall.nodePosZ, Level_WallObj.WallDirection.ab);

                    Node finalXNode = null;
                    Node finalZNode = null;

                    if (difX != 0)
                    {
                        bool xHigher = (difX > 0);

                        for(int i = 1; i <Mathf.Abs(difX) + 1;i++ )
                        {
                            int offset = xHigher ? i : -i;
                            int posX = startNode_Wall.nodePosX + offset;
                            int posZ = startNode_Wall.nodePosZ;

                            if (posX < 0)
                            {
                                posX = 0;
                            }
                            if (posX > gridBase.sizeX)
                            {
                                posX = gridBase.sizeX;
                            }
                            if (posZ < 0)
                            {
                                posZ = 0;
                            }
                            if (posZ > gridBase.sizeZ)
                            {
                                posZ = gridBase.sizeZ;
                            }

                            finalXNode = gridBase.grid[posX, posZ];
                            Level_WallObj.WallDirection targetDir = Level_WallObj.WallDirection.ab;
                            CreateWallInNode(posX, posZ, targetDir);
                        }
                        UpdateWallCorners(xHigher ? endNodeWall : startNode_Wall, true, false, false);
                        UpdateWallCorners(xHigher ? startNode_Wall : endNodeWall, false, true, false);

                    }
                    if (difZ != 0)
                    {
                        bool zHigher = (difZ > 0);

                        for (int i = 1; i < Mathf.Abs(difZ) + 1; i++)
                        {
                            int offset = zHigher ? i : -i;
                            int posX = startNode_Wall.nodePosX;
                            int posZ = startNode_Wall.nodePosZ + offset;

                            if (posX < 0)
                            {
                                posX = 0;
                            }
                            if (posX > gridBase.sizeX)
                            {
                                posX = gridBase.sizeX;
                            }
                            if (posZ < 0)
                            {
                                posZ = 0;
                            }
                            if (posZ > gridBase.sizeZ)
                            {
                                posZ = gridBase.sizeZ;
                            }

                            Level_WallObj.WallDirection targetDir = Level_WallObj.WallDirection.bc;

                            finalZNode = gridBase.grid[posX, posZ];
                            CreateWallInNode(posX, posZ, targetDir);
                        }
                        UpdateWallNode(startNode_Wall, Level_WallObj.WallDirection.bc);

                        UpdateWallCorners(zHigher ? startNode_Wall: finalZNode, false, true, false);
                        UpdateWallCorners(zHigher ? finalZNode : startNode_Wall, false, false, true);
                    }
                    if (difZ != 0 && difX != 0)
                    {
                        bool xHigher = (difX > 0);
                        bool zHigher = (difZ > 0);

                        for (int i = 1; i < Mathf.Abs(difX) + 1; i++)
                        {
                            int offset = xHigher ? i : -i;
                            int posX = startNode_Wall.nodePosX+ offset;
                            int posZ = endNodeWall.nodePosZ;

                            if (posX < 0)
                            {
                                posX = 0;
                            }
                            if (posX > gridBase.sizeX)
                            {
                                posX = gridBase.sizeX;
                            }
                            if (posZ < 0)
                            {
                                posZ = 0;
                            }
                            if (posZ > gridBase.sizeZ)
                            {
                                posZ = gridBase.sizeZ;
                            }

                            Level_WallObj.WallDirection targetDir = Level_WallObj.WallDirection.ab;
                            CreateWallInNode(posX, posZ, targetDir);
                        }
                        for (int i = 1; i < Mathf.Abs(difZ) + 1; i++)
                        {
                            int offset = zHigher ? i : -i;
                            int posX = endNodeWall.nodePosX ;
                            int posZ = startNode_Wall.nodePosZ + offset;

                            if (posX < 0)
                            {
                                posX = 0;
                            }
                            if (posX > gridBase.sizeX)
                            {
                                posX = gridBase.sizeX;
                            }
                            if (posZ < 0)
                            {
                                posZ = 0;
                            }
                            if (posZ > gridBase.sizeZ)
                            {
                                posZ = gridBase.sizeZ;
                            }

                            Level_WallObj.WallDirection targetDir = Level_WallObj.WallDirection.bc;
                            CreateWallInNode(posX, posZ, targetDir);
                        }
                        if (startNode_Wall.nodePosZ > endNodeWall.nodePosZ)
                        {
                            #region Up to down
                            manager.inSceneWalls.Remove(finalXNode.wallObj.gameObject);
                            Destroy(finalXNode.wallObj.gameObject);
                            finalXNode.wallObj = null;

                            UpdateWallNode(finalZNode, Level_WallObj.WallDirection.all);
                            UpdateWallNode(endNodeWall, Level_WallObj.WallDirection.bc);

                            if (startNode_Wall.nodePosX > endNodeWall.nodePosX)
                            {
                                #region End node is SW of Start
                                //the furthest node on the x
                                CreateWallOrUpdateNode(finalXNode, Level_WallObj.WallDirection.ab);
                                UpdateWallCorners(finalXNode, false, true, false);
                                //the end furthest to south
                                CreateWallOrUpdateNode(finalZNode, Level_WallObj.WallDirection.bc);
                                UpdateWallCorners(finalZNode, false, true, false);
                                //first node clicked
                                //destroy node and get on next to it
                                Node nextToStartNode = DestroyCurrentNodeAndGetPrevious(startNode_Wall, true);
                                UpdateWallCorners(nextToStartNode, true, false, false);
                                //the end node clicked
                                CreateWallOrUpdateNode(endNodeWall, Level_WallObj.WallDirection.all);
                                UpdateWallCorners(endNodeWall, false, true, false);
                                #endregion
                            }
                            else
                            {
                                #region End node is SE of start
                                //the furthest node on the x
                                Node beforeFinalX = DestroyCurrentNodeAndGetPrevious(finalXNode, true);
                                UpdateWallCorners(beforeFinalX, true, false, false);
                                //the end node furthest south
                                CreateWallOrUpdateNode(finalZNode, Level_WallObj.WallDirection.all);
                                UpdateWallCorners(finalZNode, false, true, false);
                                //first node clicked
                                //destroy and get on next to it
                                CreateWallOrUpdateNode(startNode_Wall, Level_WallObj.WallDirection.ab);
                                UpdateWallCorners(startNode_Wall, false, true, false);
                                //the end node
                                CreateWallOrUpdateNode(endNodeWall, Level_WallObj.WallDirection.bc);
                                UpdateWallCorners(endNodeWall, false, true, false);
                                #endregion
                            }
                            #endregion
                        }
                        else
                        {
                            #region Down to up
                            if (startNode_Wall.nodePosX > endNodeWall.nodePosX)
                            {
                                #region End node is NW of Start
                                //the furthest node on the north east
                                Node northWestNode = DestroyCurrentNodeAndGetPrevious(finalZNode, true);
                                UpdateWallCorners(northWestNode, true, false, false);
                                //the end furthest to south west
                                CreateWallOrUpdateNode(finalXNode, Level_WallObj.WallDirection.all);
                                UpdateWallCorners(finalXNode, false, true, false);
                                //first node clicked
                                //destroy node and get on next to it
                                CreateWallOrUpdateNode(startNode_Wall, Level_WallObj.WallDirection.bc);
                                UpdateWallCorners(startNode_Wall, false, true, false);
                                //the end node clicked
                                CreateWallOrUpdateNode(endNodeWall, Level_WallObj.WallDirection.ab);
                                UpdateWallCorners(endNodeWall, false, true, false);
                                #endregion
                            }
                            else
                            {
                                #region End node is NE of start
                                //the furthest node on the north west
                                CreateWallOrUpdateNode(finalZNode, Level_WallObj.WallDirection.ab);
                                UpdateWallCorners(finalZNode, false, true, false);
                                //the end node furthest south east
                                CreateWallOrUpdateNode(finalXNode, Level_WallObj.WallDirection.bc);
                                UpdateWallCorners(finalXNode, false, true, false);
                                //first node clicked
                                CreateWallOrUpdateNode(startNode_Wall, Level_WallObj.WallDirection.all);
                                UpdateWallCorners(startNode_Wall, false, true, false);
                                //the end node
                                Node nextToEndNode = DestroyCurrentNodeAndGetPrevious(endNodeWall, true);
                                UpdateWallCorners(nextToEndNode, true, false, false);
                                #endregion
                            }
                            #endregion
                        }
                    }
                    startNode_Wall = null;
                    endNodeWall = null;
                            
                }
            }
        }
        
        void CreateWallOrUpdateNode(Node getNode, Level_WallObj.WallDirection direction)
        {
            if (getNode.wallObj == null)
            {
                CreateWallInNode(getNode.nodePosX, getNode.nodePosZ, direction);
            }
            else
            {
                UpdateWallNode(getNode, direction);
            }
        }

        Node DestroyCurrentNodeAndGetPrevious(Node curNode, bool positive)
        {
            int i = (positive) ? 1 : -1;

            Node beforeCurNode = gridBase.grid[curNode.nodePosX - i, curNode.nodePosZ];

            if (curNode.wallObj != null)
            {
                if (manager.inSceneWalls.Contains(curNode.wallObj.gameObject))
                {
                    manager.inSceneWalls.Remove(curNode.wallObj.gameObject);
                    Destroy(curNode.wallObj.gameObject);
                    curNode.wallObj = null;
                }
            }
            return beforeCurNode;
        }

        void CreateWallInNode(int posX,int posZ, Level_WallObj.WallDirection direction)
        {
            Node getNode = gridBase.grid[posX, posZ];
            Vector3 wallPosition = getNode.vis.transform.position;
            if (getNode.wallObj == null)
            {
                GameObject actualObjPlaced = Instantiate(wallPrefab, wallPosition, Quaternion.identity) as GameObject;
                Level_Object placedObjProperties = actualObjPlaced.GetComponent<Level_Object>();
                Level_WallObj placedWallProperties = actualObjPlaced.GetComponent<Level_WallObj>();

                placedObjProperties.gridPosX = posX;
                placedObjProperties.gridPosZ = posZ;
                manager.inSceneWalls.Add(actualObjPlaced);
                getNode.wallObj = placedWallProperties;

                UpdateWallNode(getNode, direction);
            }
            else
            {
                UpdateWallNode(getNode, direction);
            }
            UpdateWallCorners(getNode, false, false, false);
        }

        void UpdateWallNode(Node getNode, Level_WallObj.WallDirection direction)
        {
            // for loop not neccessary??
            //for (int i = 0; i<getNode.wallObj.wallsList.Count;i++)
           // {
                getNode.wallObj.UpdateWall(direction);
           // }
        }

        void UpdateWallCorners(Node getNode, bool a,bool b,bool c)
        {
            if(getNode.wallObj != null)
            {
                getNode.wallObj.UpdateCorners(a, b, c);
            }
        }

        public void DeleteWall()
        {
            CloseAll();
            deleteWall = true;
        }

        void DeleteWallsActual()
        {
            if (deleteWall)
            {
                UpdateMousePosition();

                Node curNode = gridBase.NodeFromWorldPosition(mousePosition);

                if (Input.GetMouseButton(0) && !ui.mouseOverUIElement)
                {
                    if (curNode.wallObj != null)
                    {
                        if (manager.inSceneWalls.Contains(curNode.wallObj.gameObject))
                        {
                            manager.inSceneWalls.Remove(curNode.wallObj.gameObject);
                            Destroy(curNode.wallObj.gameObject);
                        }
                        curNode.wallObj = null;
                    }
                }
            }
        }

        #endregion


        void CloseAll()
        {
            hasObj = false;
            deleteObj = false;
            paintTile = false;
            placeStackObj = false;
            createWall = false;
            hasMaterial = false;
            deleteStackObj = false;
            deleteWall = false;
        }
    }
}
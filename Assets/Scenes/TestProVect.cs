using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestProVect : MonoBehaviour
{
    public Vector3 oldTargetPos = new Vector3(-9517.30957f, -2025.72998f, 390.570007f);
    public Vector3 newTargetPos = new Vector3(-9517.30957f, 0, 390.570007f);
    public Vector3 direct = new Vector3(-0.98f, -0.21f, 0.04f);
    public Vector3 origin = new Vector3(255.26f, 52.84f, -29.58f);
    public float totalDis = 10000;

    public GameObject objA;
    public GameObject objB;
    public GameObject objC;
    public LineRenderer line;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetRepairCenterPoint(oldTargetPos, newTargetPos, direct, origin, totalDis);
    }

    //public Vector3 GetRepairCenterPoint(Vector3 oldTargetPoint, Vector3 newTargetPoint, Vector3 direct, Vector3 origin, float totalDis)
    //{
    //    Vector3d pos_A = oldTargetPoint.ToVector3d();
    //    //Vector3d pos_B = Point3DToPlane(pos_A, Vector3d.zero, -Vector3d.up);
    //    Vector3d pos_B = newTargetPoint.ToVector3d();
    //    Vector3d direct_3d = direct.ToVector3d();
    //    Vector3d origin_3d = origin.ToVector3d();

    //    // ��� pos_A �� pos_B �Ƿ���ͬ
    //    //pos_B = PointToPlane(pos_A, Vector3.zero, Vector3.down);
    //    Debug.LogError($"pos_A:{pos_A}||pos_B:{pos_B}|totalDis:{totalDis}");
    //    if (pos_B == pos_A)
    //    {
    //        Debug.LogError("pos_A and pos_B must be different.");
    //        return pos_A.ToVector3();
    //    }


    //    //������֪�����������ļнǣ�����һ��ˮƽ��90�ȣ�һ�������ĳ��ȣ�������һ�������ĳ���
    //    double disA = Vector3d.Distance(pos_A, pos_B);
    //    //��ray��old,new���ߵļн�˳ʱ��н�
    //    double angle_a = Vector3d.Angle(pos_B - pos_A, -direct_3d);
    //    // ��� angle_a Ϊ 180 �ȣ�����Ϊ 0 �� c=100/sin(90-77��)
    //    //if (angle_a == 180f)
    //    //{
    //    //    angle_a = 0f;
    //    //}
    //    //����һ��ֱ�������Σ��������Ҷ������������һ��ֱ�Ǳ߳�
    //    double disC = disA / Math.Sin(DegreesToRadians(90 - angle_a)); //sin������Ҫ����ֵ
    //                                                            //���ݹ��ɶ������������һ��ֱ�Ǳ߳�
    //    //float disC = Mathf.Sqrt(disA * disA + disB * disB);
    //    //Vector3 fixPos = ray.GetPoint(totalDis - disC);
    //    Vector3d fixPos = origin_3d + direct_3d * (totalDis - (disC));
    //    Debug.LogError($"disA:{disA}|disC:{disC}|angle_a:{angle_a}|totalDis:{totalDis}|fixDis:{totalDis - disC}|fixPos:{fixPos}");
    //    //// ȷ�� totalDis ���ڵ��� disC
    //    //if (totalDis < disC)
    //    //{
    //    //    Debug.LogError("totalDis must be greater than or equal to disC.");
    //    //    return pos_A;
    //    //}
    //    objA.transform.position = pos_A.ToVector3();
    //    objB.transform.position = pos_B.ToVector3();
    //    objC.transform.position = fixPos.ToVector3();
    //    line.SetPosition(0, pos_A.ToVector3());
    //    line.SetPosition(1, pos_B.ToVector3());
    //    //line.SetPosition(3, pos_A);
    //    line.SetPosition(2, fixPos.ToVector3());

    //    return fixPos.ToVector3();
    //}

    /// <summary>
    /// Converts an angle from degrees to radians.
    /// </summary>
    /// <param name="degrees">The angle in degrees.</param>
    /// <returns>The angle in radians.</returns>
    public static double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }



    
    Vector3 PointToPlane(Vector3 worldpos, Vector3 targetPlanePos, Vector3 planeNormal)
    {
        var localpos = worldpos - targetPlanePos;
        var dis = Vector3.Dot(localpos, planeNormal);
        var vecN = planeNormal * dis;
        return worldpos - vecN;
    }


    public Vector3 GetRepairCenterPoint(Vector3 oldTargetPoint, Vector3 newTargetPoint, Vector3 direct, Vector3 origin, float totalDis)
    {
        // ��� oldTargetPoint �� newTargetPoint �Ƿ���ͬ
        //newTargetPoint = PointToPlane(oldTargetPoint, Vector3.zero, Vector3.down);
        Debug.LogError($"oldTargetPoint:{oldTargetPoint}||newTargetPoint:{newTargetPoint}|totalDis:{totalDis}");
        if (newTargetPoint == oldTargetPoint)
        {
            Debug.LogError("oldTargetPoint and newTargetPoint must be different.");
            return oldTargetPoint;
        }


        //������֪�����������ļнǣ�����һ��ˮƽ��90�ȣ�һ�������ĳ��ȣ�������һ�������ĳ���
        double disA = Vector3.Distance(oldTargetPoint, newTargetPoint);
        //��ray��old,new���ߵļн�˳ʱ��н�
        double angle_a = Vector3.Angle(newTargetPoint - oldTargetPoint, -direct);
        // ��� angle_a Ϊ 180 �ȣ�����Ϊ 0 �� c=100/sin(90-77��)
        //if (angle_a == 180f)
        //{
        //    angle_a = 0f;
        //}
        //����һ��ֱ�������Σ��������Ҷ������������һ��ֱ�Ǳ߳�
        double disC = disA / Math.Sin(DegreesToRadians(90 - angle_a)); //sin������Ҫ����ֵ
                                                                       //���ݹ��ɶ������������һ��ֱ�Ǳ߳�
                                                                       //float disC = Mathf.Sqrt(disA * disA + disB * disB);
                                                                       //Vector3 fixPos = ray.GetPoint(totalDis - disC);
        Vector3 fixPos = origin + direct.normalized * (totalDis - Convert.ToSingle(disC));
        Debug.LogError($"disA:{disA}|disC:{disC}|angle_a:{angle_a}|totalDis:{totalDis}|fixDis:{totalDis - disC}|fixPos:{fixPos}");
        //// ȷ�� totalDis ���ڵ��� disC
        //if (totalDis < disC)
        //{
        //    Debug.LogError("totalDis must be greater than or equal to disC.");
        //    return oldTargetPoint;
        //}
        objA.transform.position = oldTargetPoint;
        objB.transform.position = newTargetPoint;
        objC.transform.position = fixPos;
        line.SetPosition(0, oldTargetPoint);
        line.SetPosition(1, newTargetPoint);
        //line.SetPosition(3, oldTargetPoint);
        line.SetPosition(2, fixPos);

        return fixPos;
    }
}

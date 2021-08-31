using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

class Avatar {
    public GameObject obj;
    public IAvatar controller;

    public Avatar(GameObject obj, IAvatar controller) {
        this.obj = obj;
        this.controller = controller;
    }

    public static Avatar FromGameObject(GameObject obj) {
        return new Avatar(obj, obj.GetComponent<IAvatar>());
    }
}

public class AvatarController : MonoBehaviour
{
    const string ToggleKey = "q";

    Avatar[] avatars;

    // Avatar control interface
    Thread receiveThread;
    TcpClient client;
    TcpListener listener;
    int port = 5066;

    // Avatar state data
    private int currentAvatar;
    private float roll = 0, pitch = 0, yaw = 0;
    private float x_ratio_left = 0, y_ratio_left = 0, x_ratio_right = 0, y_ratio_right = 0;
    private float ear_left = 0, ear_right = 0;
    private float mar = 0;

    // Launch TCP to receive message from python
    private void InitTCP() {
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData() {
        try {
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            listener.Start();
            Byte[] bytes = new Byte[1024];

            while (true) {
                using(client = listener.AcceptTcpClient()) {
                    using (NetworkStream stream = client.GetStream()) {
                        int length;
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            string clientMessage = Encoding.ASCII.GetString(incommingData);
                            string[] res = clientMessage.Split(' ');

                            AvatarState state = new AvatarState();
                            state.roll = float.Parse(res[0]);
                            state.pitch = float.Parse(res[1]);
                            state.yaw = float.Parse(res[2]);
                            state.ear_left = float.Parse(res[3]);
                            state.ear_right = float.Parse(res[4]);
                            state.x_ratio_left = float.Parse(res[5]);
                            state.y_ratio_left = float.Parse(res[6]);
                            state.x_ratio_right = float.Parse(res[7]);
                            state.y_ratio_right = float.Parse(res[8]);
                            state.mouth_aspect_ratio = float.Parse(res[9]);
                            state.mouth_dist = float.Parse(res[10]);

                            avatars[currentAvatar].controller.state = state;
                        }
                    }
                }
            }
        } catch(Exception e) {
            Debug.LogException(e, this);
        } finally {
            listener?.Stop();
        }
    }

    void OnApplicationQuit()
    {
        // close the thread when the application quits
        receiveThread.Abort();
    }

    private void SwitchAvatar(int newIndex) {
        currentAvatar = newIndex;
        for (var i = 0; i < avatars.Length; i++) {
            var avatar = avatars[i];
            avatar.obj.SetActive(i == newIndex);
        }
    }

    void Start()
    {
        if (avatars == null) {
            var foundAvatars  = GameObject.FindGameObjectsWithTag("Avatar");
            avatars = new Avatar[foundAvatars.Length];
            Debug.LogFormat("Found {0} avatars", avatars.Length);
            for (var i = 0; i < foundAvatars.Length; i++) {
                avatars[i] = Avatar.FromGameObject(foundAvatars[i]);
                Debug.LogFormat("Avatar: {0}", avatars[i].obj.name);
            }
        }

        SwitchAvatar(0);

        InitTCP();
    }

    void Update()
    {
        if (!Input.GetKeyDown(ToggleKey)) {
            return;
        }

        SwitchAvatar((currentAvatar + 1) % avatars.Length);
    }
}

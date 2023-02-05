using UnityEngine;
using Fusion;
using TMPro;

public class Character : NetworkBehaviour
{
    [SerializeField]
    private NetworkCharacterControllerPrototype cc;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private TMP_Text nickNameText;

    [Networked(OnChanged = nameof(OnNickNameChanged))]
    NetworkString<_16> NickName {get; set;}
    [Networked]
    public NetworkButtons Prevbuttons {get; set;}

    private NetworkButtons buttons;
    private NetworkButtons pressed;
    private NetworkButtons released;
    private Vector2 inputDir;
    private Vector3 moveDir;

    public override void Spawned()
    {
        if(!Object.HasInputAuthority){
            Destroy(cam.gameObject);
            return;
        }

        cam.gameObject.SetActive(true);
        RPC_SendNickName("Name: "+Random.Range(0, 100).ToString());
    }

    public override void Render()
    {
        if(!Object.HasInputAuthority){
            return;
        }
        cam.transform.localRotation = Quaternion.Euler(Mathf.Clamp(NetworkCallback.nc.Pitch, -80, 80), 0, 0);
    }

    public override void FixedUpdateNetwork()
    {
        buttons = default;

        if (GetInput<NetworkInputData>(out var input)){
            buttons = input.buttons;
        }

        pressed = buttons.GetPressed(Prevbuttons);
        released = buttons.GetReleased(Prevbuttons);

        Prevbuttons = buttons;

        inputDir = Vector2.zero;

        if(buttons.IsSet(Buttons.forward)){
            inputDir += Vector2.up;
        }
        if(buttons.IsSet(Buttons.back)){
            inputDir += Vector2.down;
        }
        if(buttons.IsSet(Buttons.right)){
            inputDir += Vector2.right;
        }
        if(buttons.IsSet(Buttons.left)){
            inputDir += Vector2.left;
        }

        if(pressed.IsSet(Buttons.jump)){
            cc.Jump();
        }

        moveDir = transform.forward*inputDir.y + transform.right*inputDir.x;

        cc.Move(moveDir);

        transform.rotation = Quaternion.Euler(0, (float)input.yaw, 0);
    }

    public static void OnNickNameChanged(Changed<Character> changed){
        changed.Behaviour.SetNickName();
    }

    public void SetNickName(){
        nickNameText.text = NickName.Value;
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_SendNickName(NetworkString<_16> message){
        NickName = message;
    }
}



function MQTTSendToUnityCommon(method, data){
    myUnityInstance.SendMessage('ServerMgr', method, data)
    return jsonStr;
}
function MQTTSendToUnity(typeInfo, obj){
    let sendTable = {
        Code: typeInfo,
        Data: obj,
    }
    let jsonStr = JSON.stringify(sendTable);
    myUnityInstance.SendMessage('ServerMgr', 'SetJSMessage', jsonStr)
}

function GetTransData(code, obj){
    let sendTable = {
        Code: code,
        Data: obj,
    }
    let jsonStr = JSON.stringify(sendTable);
    return jsonStr;
}

function getUrlParams(){
    let url = window.location.href;
	let ItemArr=[];
    let ItemObj= {};
    url
      .substring(url.lastIndexOf("?") + 1, url.length)
      .split("&")
      .map((item) => {
        ItemArr.push(item.split("="));
      });
    ItemArr.map((item) => {
      ItemObj[item[0]] = item[1];
    });
	return JSON.stringify(ItemObj);
}

function GetWS(){
    return WebSocket;
}


var rec = null;
/**
 * 开始录音
 *
 * @returns 无返回值
 */
function Recorder_Start(){
    // Recorder.ConnectEnableWorklet = true
    Recorder_SendConfig(); // 发送配置数据
    // 录音; 定义录音对象,wav格式
    rec = Recorder({
        type: "pcm",
        bitRate: 16,
        sampleRate: 16000,
        onProcess: recProcess
    });
    
    rec.open(function() {
        rec.start();
        console.log("开始录音");
    });
}

//发送recorder配置数据
function Recorder_SendConfig(){
    // 发送json
    var chunk_size = new Array( 5, 10, 5 );
    var request = {
        "chunk_size": chunk_size,
        "wav_name":  "h5",
        "is_speaking":  true,
        "chunk_interval":10,
        "itn":false,
        "mode":"2pass", // 2pass, online, offline
        
    };
    // if(isfilemode) //麦克风录音false
    // {
    //     request.wav_format=file_ext;
    //     if(file_ext=="wav")
    //     {
    //         request.wav_format="PCM";
    //         request.audio_fs=file_sample_rate;
    //     }
    // }
    
    var hotwords=getHotwords();

    if(hotwords!=null  )
    {
        request.hotwords=hotwords;
    }
    console.log(JSON.stringify(request));
    SocketSendArray(JSON.stringify(request));
}

function getHotwords(){
    var hotwords="哈哈哈哈 10\n 你好 20\n小智 10测试 20\n";
	let val = hotwords;
  
	console.log("hotwords="+val);
	let items = val.split(/[(\r\n)\r\n]+/);  //split by \r\n
	var jsonresult = {};
	const regexNum = /^[0-9]*$/; // test number
	for (item of items) {
  
		let result = item.split(" ");
		if(result.length>=2 && regexNum.test(result[result.length-1]))
		{ 
			var wordstr="";
			for(var i=0;i<result.length-1;i++)
				wordstr=wordstr+result[i]+" ";
  
			jsonresult[wordstr.trim()]= parseInt(result[result.length-1]);
		}
	}
	console.log("jsonresult="+JSON.stringify(jsonresult));
	return  JSON.stringify(jsonresult);

}

/**
 * 停止录音
 *
 * @returns 无返回值
 */
function Recorder_Stop(){
    rec.stop(function(blob, duration) {
        console.log(blob);
        // var audioBlob = Recorder.pcm2wav(data = {sampleRate:16000, bitRate:16, blob:blob},
        //     function(theblob, duration) {
        //         console.log(theblob);
        //         var audio_record = document.getElementById('audio_record');
        //         audio_record.src = (window.URL || webkitURL).createObjectURL(theblob);
        //         audio_record.controls = true;
        //         //audio_record.play();
        //     }, function(msg) {
        //         console.log(msg);
        //     }
        // );
    }, function(errMsg) {
        console.log("errMsg: " + errMsg);
    });
}

var socketInstance = null;
function SocketSendArray(data)
{
	// var socket = socketInstance[0];
	socketInstance.send (data);
    // socketInstance.SocketSendArray(data);
}

/**
 * 对缓冲区进行音频处理
 *
 * @param buffer 缓冲区数组
 * @param powerLevel 功率级别
 * @param bufferDuration 缓冲区时长
 * @param bufferSampleRate 缓冲区采样率
 * @param newBufferIdx 新缓冲区索引
 * @param asyncEnd 是否异步结束
 * @returns 无返回值
 */
function recProcess(buffer, powerLevel, bufferDuration, bufferSampleRate, newBufferIdx, asyncEnd) {
    if (true) {
        var sampleBuf = new Int16Array();
        var data_48k = buffer[buffer.length - 1];

        var array_48k = new Array(data_48k);
        var data_16k = Recorder.SampleData(array_48k, bufferSampleRate, 16000).data;

        sampleBuf = Int16Array.from([...sampleBuf, ...data_16k]);
        var chunk_size = 960; // for asr chunk_size [5, 10, 5]
        // info_div.innerHTML = "" + bufferDuration / 1000 + "s";
        while (sampleBuf.length >= chunk_size) {
            sendBuf = sampleBuf.slice(0, chunk_size);
            sampleBuf = sampleBuf.slice(chunk_size, sampleBuf.length);
            // wsconnecter.wsSend(sendBuf);
            console.info("sendBuf: "+ typeof(sendBuf) + " " + sendBuf.length);
            SocketSendArray(sendBuf);
        }
    }
}

function Int16ArrayToBase64(int16Array) {
    // 将 Int16Array 转换为 Uint8Array
    const uint8Array = new Uint8Array(new ArrayBuffer(int16Array.byteLength));
    const view = new DataView(uint8Array.buffer);
    for (let i = 0; i < int16Array.length; i++) {
        view.setInt16(i * 2, int16Array[i], false); // 小端模式
    }

    // 将 Uint8Array 转换为 Base64 字符串
    const binaryString = new TextEncoder().encode(uint8Array).reduce((acc, val) => acc + String.fromCharCode(val), '');
    return btoa(binaryString);
}

function JS_SendToUnityCommon(method, data) {
    myUnityInstance.SendMessage('LLWebGLMicrophone', method, data);
}


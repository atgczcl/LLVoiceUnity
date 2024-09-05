mergeInto(LibraryManager.library, {

    JS_Microphone_Start: function() {
        Recorder_Start();
    },

    

    JS_Microphone_Stop: function() {
        Recorder_Stop();
    },

    JS_Microphone_IsCanSendData: function(is_cansend)
    {
        SetIsCanSendData(is_cansend);
    }

});


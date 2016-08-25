package websocket.websocketapp;

import android.app.Activity;
import android.os.AsyncTask;
import android.os.Bundle;
import android.util.JsonWriter;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

import org.java_websocket.client.WebSocketClient;
import org.java_websocket.handshake.ServerHandshake;
import org.json.JSONException;
import org.json.JSONObject;

import java.net.URI;
import java.net.URISyntaxException;
import java.nio.charset.Charset;

import org.java_websocket.client.*;

public class RegisterActivity extends Activity {

    private WebSocketClient mWebSocketClient;
    private TCPClient mTcpClient;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_register);

        Button registerBtn = (Button)findViewById(R.id.register_button);
        registerBtn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {

                EditText editText = (EditText)findViewById(R.id.editText);
                String username = editText.getText().toString();
                registerUser(username);
            }
        });
    }

    private void registerUser(String username)
    {
        // connect to the server
        new connectTask(username).execute();
    }

    public class connectTask extends AsyncTask<String,String,TCPClient> {

        String username = null;
        public connectTask(String username)
        {
            this.username = username;
        }

        @Override
        protected TCPClient doInBackground(final String... message) {

            //we create a TCPClient object and
            mTcpClient = new TCPClient(new TCPClient.OnMessageReceived() {
                @Override
                //here the messageReceived method is implemented
                public void messageReceived(final String message) {

                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {

                            try
                            {
                                JSONObject user = new JSONObject(message);
                                Toast.makeText(getApplicationContext(), user.getString("username"), Toast.LENGTH_LONG).show();
                            }
                            catch (JSONException j)
                            {
                                Toast.makeText(getApplicationContext(), j.getMessage(), Toast.LENGTH_LONG).show();
                            }

                            mTcpClient.closeConnection();
                            mTcpClient = null;
                        }


                    });
                }
            });
            mTcpClient.connect();
            runOnUiThread(new Runnable() {

                @Override
                public void run() {
                    JSONObject sendRegister = new JSONObject();
                    try{
                        sendRegister.put("username", username);
                        mTcpClient.sendMessage(sendRegister);
                    }catch(JSONException e){
                        Toast.makeText(getApplicationContext(), "JSON error!", Toast.LENGTH_LONG).show();
                    }
                }
            });
            mTcpClient.pollForServerMesssages();
            return null;
        }
    }
}

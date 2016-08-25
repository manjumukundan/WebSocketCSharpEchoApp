package websocket.websocketapp;

import android.util.Log;

import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.InetAddress;
import java.net.Socket;
import java.net.SocketAddress;

/**
 * Created by t_mukum on 8/19/2016.
 */
public class TCPClient
{
    private OnMessageReceived mMessageListener = null;
    private boolean mRun = false;
    private PrintWriter out;
    private BufferedReader in;
    private String serverMessage;
    private Socket socket = null;

    public TCPClient(OnMessageReceived listener) {
        mMessageListener = listener;
    }

    public void stopClient(){
        mRun = false;
    }

    public void sendMessage(JSONObject message){
        if (out != null && !out.checkError()) {
            out.println(message);
            out.flush();
        }
    }

    public void connect ()
    {
            try
            {
                //here you must put your computer's IP address.
                InetAddress serverAddr = InetAddress.getByName("192.168.0.8");

                //create a socket to make the connection with the server
                socket = new Socket(serverAddr, 3000);


                //send the message to the server
                out = new PrintWriter(new BufferedWriter(new OutputStreamWriter(socket.getOutputStream())), true);


                //receive the message which the server sends back
                in = new BufferedReader(new InputStreamReader(socket.getInputStream()));

            }
            catch (Exception e) {

                Log.e("TCP", "C: Error", e);

            }
    }
    public void pollForServerMesssages()
    {
        try
        {
            //in this while the client listens for the messages sent by the server
            while (true)
            {
                serverMessage = in.readLine();

                if (serverMessage != null && mMessageListener != null)
                {
                    //call the method messageReceived from MyActivity class
                    mMessageListener.messageReceived(serverMessage);
                }
            }
        }
        catch (Exception e) {

            Log.e("TCP", "C: Error", e);

        }
    }

    public void closeConnection()
    {
        try
        {
            socket.close();
        }
        catch (Exception e)
        {
            Log.e("TCP", "socket close error", e);
        }
    }


    public interface OnMessageReceived {
        public void messageReceived(String message);
    }
}

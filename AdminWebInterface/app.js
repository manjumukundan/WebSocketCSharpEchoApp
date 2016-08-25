window.onload = function() {

  // Get references to elements on the page.
  var socketStatus = document.getElementById('status');


  // Create a new WebSocket.
  var socket = new WebSocket('ws://192.168.0.8:3000');


  // Handle any errors that occur.
  socket.onerror = function(error) {
    console.log('WebSocket Error: ' + error);
  };


  // Show a connected message when the WebSocket is opened.
  socket.onopen = function(event) {
    socketStatus.innerHTML = 'Connected';
    socketStatus.className = 'open';
	socket.send('OK');
  };


  // Handle messages sent by the server.
  socket.onmessage = function(event) {
   // var message = event.;
    socketStatus.innerHTML = 'Echo Send';
  };

  // Show a disconnected message when the WebSocket is closed.
  socket.onclose = function(event) {
    socketStatus.innerHTML = 'Disconnected from WebSocket.';
    socketStatus.className = 'closed';
  };

};

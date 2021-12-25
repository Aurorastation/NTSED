var programId = null;
window.vueapp = {};
function console_log(o) {
  if (typeof o != "string") o = JSON.stringify(o);
  $("#output").append(`[${Date()}] ${o}\r\n`);
}

function create_program_com() {
  if (programId != null) return;
  axios({
    url: "/new_program",
    method: "get",
    params: {
      type: "Computer",
    },
  }).then((res) => {
    console_log("Program id: " + res.data);
    programId = res.data;
    get_buffer();
  });
}

// function create_program_tcom() {
//   if (programId != null) return;
//   $.ajax({
//     type: "GET",
//     url: "/new_program",
//     data: {
//       type: "TCom",
//     },
//     success: (data) => {
//       console_log("Program id: " + data);
//       programId = data;
//     },
//   });
// }

function exec_program() {
  if (programId == null) return;
  axios({
    url: "/execute",
    method: "post",
    params: {
      id: programId,
      scriptName: "testing.nts",
    },
    headers: { "Content-Type": "application/json" },
    data: JSON.stringify(editor.getValue()),
  }).then((res) => {
    console_log(res.data);
    get_buffer("Execute: " + res.data);
  });
}

function remove_program() {
  if (programId == null) return;
  axios({
    url: "/remove",
    method: "get",
    params: {
      id: programId,
    },
  }).then((res) => {
    window.vueapp.$data.buffer = "";
    $("#output").text("");
    console_log("Program removed. " + res.data);
    programId = null;
  });
}

function get_buffer() {
  if (programId == null) return;
  axios({
      url: "/terminal/get_buffer",
    method: "get",
    params: {
      id: programId,
    },
  }).then((res) => {
    window.vueapp.$data.buffer = res.data;
  });
}

function reset() {
  if (programId == null) return;
  axios({
    url: "/clear",
    method: "get",
  }).then((res) => {
    window.vueapp.$data.buffer = "";
    $("#output").text("");
    programId = null;
    console_log(res.data);
  });
}

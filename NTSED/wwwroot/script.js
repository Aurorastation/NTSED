const co = {
  props: ["fg", "bg"],
  computed: {
    style() {
      return {
        color: "#" + this.fg,
        "background-color": "#" + this.bg,
      };
    },
  },
  template: '<span :style="style"><slot/></span>',
};
const to = {
  props: ["to"],
  methods: {
    invoke() {
      if (programId == null) return;
      var topic = this.to;
      var data = "";
      if (this.to[0] == "?") {
        topic = this.to.substring(1);
        data = prompt("Please enter input...", "");
      }
      axios({
          url: "/terminal/topic",
        method: "post",
        params: {
          id: programId,
          topic: topic,
        },
        headers: { "Content-Type": "application/json" },
        data: JSON.stringify(data),
      }).then(() => get_buffer());
    },
  },
  template:
    '<span style="cursor: pointer;" @click.prevent="invoke"><slot/></span>',
};

window.vueapp = new Vue({
  el: "#consolebuffer",
  template: '<component class="terminal" :is="term"/>',
  data: {
    buffer: "",
  },
  computed: {
    term() {
      return {
        components: {
          co,
          to,
        },
        template: `<div>${this.buffer}</div>`,
      };
    },
  },
});

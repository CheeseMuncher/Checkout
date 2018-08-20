import constants from '/config.js';
let skus_endpoint = constants.skus_endpoint;

Vue.prototype.$eventHub = new Vue(); // Global event bus
new Vue({
    el: '#skus',
    data () {                
        return {
            message: null,
            selectedSku: null,
            availableSkus: []
        }
    },
    mounted(){
        axios
        .get(skus_endpoint)
        .then(response => {
            this.availableSkus = response.data;
            if(this.availableSkus.length > 0)
            {
                this.selectedSku = this.availableSkus[0];
            }
        })
        .catch(function (error) {
            console.log(error.message);
            this.message = error.message
        })
    },
    methods : {
        addSku : function(code){
            this.$eventHub.$emit('addSku', code);
        }
    }
});
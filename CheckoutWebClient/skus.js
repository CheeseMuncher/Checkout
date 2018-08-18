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
        var header = {
            'Access-Control-Allow-Origin': '*',
            'Access-Control-Allow-Methods': 'GET, PUT, POST, DELETE, OPTIONS',
            'Content-Type': 'application/json',
        }
        axios
        .get('https://localhost:44315/api/skus', { crossdomain: true })
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
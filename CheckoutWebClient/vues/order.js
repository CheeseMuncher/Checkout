import constants from '/config.js';
let orders_endpoint = constants.orders_endpoint;

new Vue({
    el: '#order',
    data () {                
        return {
            message: null,
            orderId: null,
            lines: [],
        }
    },
    created() { 
        var uri = window.location.search.substring(1); 
        var params = new URLSearchParams(uri);
        if(params.has("id")) {
            this.orderId = params.get("id");
        }
        else {
            this.message = "No Order id specified"
        }
    },
    mounted () {
        var vm = this;
        this.$eventHub.$on('addSku', function (payload) {
            function checkSku(line){
                return line.skuCode === payload.code;
            };
            if(vm.lines.some(checkSku)) {
                vm.message = "Order already contains a line with this product";
            }
            else {
                vm.message = "";
                axios
                .put(orders_endpoint + vm.orderId, {
                    "quantity": 1,
                    "skuCode": payload.code
                }).then(response => {
                    vm.message = "";
                    var max = function(lines){
                        return Math.max(...lines.map(line => line.sortOrder))
                    }
                    var newLine = {
                        id: response.data,
                        quantity: 1,
                        skuCode: payload.code,
                        skuDisplayName: payload.displayName,
                        sortOrder: max(vm.lines) + 10                            
                    }
                    vm.lines.push(newLine)
                })
                .catch(function (error) {
                    console.log(error.message);
                    vm.message = error.response.status == 500 ? error.response.statusText : error.response.data;
                })
            }
        });            
        if (this.orderId != null) {
            axios
            .get(orders_endpoint + this.orderId)
            .then(response => {
                vm.message = "";
                this.lines = response.data.lines;
            })
            .catch(function (error) {
                console.log(error.message);
                vm.message = error.response.data; // TODO: this.message = doesn't update the view
            })
        }
    },
    methods : {
        updateQuantity : function(line){
            this.message = "";
            axios // TODO avoid invoking the server if button clicked but value hasn't changed
            .put(orders_endpoint + this.orderId, {
                "id": line.id,
                "quantity": line.quantity,
                "skuCode": line.skuCode
            }).then(response => { })
            .catch(function (error) {
                console.log(error.message);
                this.message = error.response.status == 500 ? error.response.statusText : error.response.data;
            })
        },
        clearLine : function(line){
            this.message = "";
            axios
            .delete(orders_endpoint + this.orderId + "?orderLineId=" + line.id)
            .then(response => {
                index = this.lines.findIndex(l => l.skuCode === line.skuCode);
                this.lines.splice(index, 1);
             })
            .catch(function (error) {
                console.log(error.message);
                this.message = error.response.status == 500 ? error.response.statusText : error.response.data;
            })
        },
        clearOrder : function(){
            this.message = "";
            axios
            .delete(orders_endpoint + this.orderId)
            .then(response => {
                this.lines = [];
             })
            .catch(function (error) {
                console.log(error.message);
                this.message = error.response.status == 500 ? error.response.statusText : error.response.data;
            })
        }
    }
});

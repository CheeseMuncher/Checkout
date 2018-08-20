import constants from '/config.js';
let orders_endpoint = constants.orders_endpoint;

new Vue({
    el: '#orders',
    data () {                
        return {
            error: false,
            message: null,
            myOrders: []
        }
    },
    mounted () {
        var vm = this;
        axios
        .get(orders_endpoint)
        .then(response => {
            this.myOrders = response.data;
        })
        .catch(function (error) {
            vm.error = true;
            console.log(error.message);
            vm.message = error.message
        })
    },
    methods : {
        getOrder: function(id, event) {
            this.error = false;
            this.message = "Fetching order with Id " + id;
            window.location.href = "order.html?id=" + id 
        },
        newOrder: function(){
            var vm = this;
            vm.error = false;
            vm.message = "Creating order...";
            axios
            .post(orders_endpoint, {
                    "id": 0,
                    "lines": []
            })
            .then(response => {     
                window.location.href = "order.html?id=" + response.data
            })
            .catch(function (error) {
                console.log(error.message);
                vm.error = true;
                vm.message = error.response.request.statusText;
            })
        }
    }
});
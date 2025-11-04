import { defineStore } from "pinia";

export const useLoadingStore = defineStore('loading',{
    state: () =>({
        map: {}, 
    }),
    getters:{
        isActive: (s) => (key) =>{
            if (key) return (s.map[key] ?? 0) > 0;
            return Object.values(s.map).some(v => v > 0);
        },
    },
    actions:{
        start(key = 'global'){
            this.map[key] = (this.map[key] ?? 0) + 1;
        },
        stop(key='global'){
            const next = (this.map[key] ?? 0) - 1;
            if(next <= 0) delete this.map[key];
            else this.map[key] = next;
        },
        clear(key){
            if (key) delete this.map[key];
            else this.map = {};
        },
    },
});
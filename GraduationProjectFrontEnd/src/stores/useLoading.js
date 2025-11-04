import { computed } from "vue";
import { useLoadingStore } from "./loading";

export function useLoading(scope){
    const s = useLoadingStore();
    const active = computed(() => s.isActive(scope));
    const start = () => s.start(scope);
    const stop = () => s.stop(scope);

    const withLoading = async (fn) =>{
        start();
        try{
            return await fn();
        }finally{
            stop();
        }
    };

    return {active, start, stop, withLoading};
}
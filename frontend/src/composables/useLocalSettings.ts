import { ref } from "vue";

const showGraph = ref(localStorage.getItem("showGraph") !== "false");
const refetchInterval = ref(Number(localStorage.getItem("refetchInterval")) || 5000);

export function useLocalSettings() {
  const setShowGraph = (value: boolean) => {
    showGraph.value = value;
    localStorage.setItem("showGraph", String(value));
  };

  const toggleGraph = () => {
    setShowGraph(!showGraph.value);
  };

  const setRefetchInterval = (value: number) => {
    refetchInterval.value = value;
    localStorage.setItem("refetchInterval", String(value));
  };

  return {
    showGraph,
    refetchInterval,
    setShowGraph,
    toggleGraph,
    setRefetchInterval,
  };
}

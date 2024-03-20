<template>
  <div class="wrapper">
    <div class="module-border-wrap">
      <div class="module">Your Balance is {{ data }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { getMyBalance } from "@/core/api/graphql/customPage/queries";
const data = ref<number | null>(null);
onMounted(async () => {
  const apiData = await getMyBalance({ storeId: "B2B-store" });
  data.value = apiData.mybalance.balance;
});
</script>
<style scoped>
.module-border-wrap {
  max-width: 250px;
  padding: 1rem;
  position: relative;
  background: linear-gradient(to right, red, purple);
  padding: 3px;
}

.module {
  background: #222;
  color: white;
  padding: 2rem;
}
.wrapper {
  margin-top: 20px;
  display: flex;
  justify-content: center;
  align-items: center;
}
</style>

<template>
  <div v-if="order">
    <BackButtonInHeader v-if="isMobile" @click="$router.back()" />

    <VcBreadcrumbs :items="breadcrumbs" class="hidden lg:block" />

    <!-- Title block -->
    <div class="mx-7 flex flex-col gap-2.5 print:mx-0 lg:mx-0 lg:flex-row lg:justify-between">
      <VcTypography tag="h1" variant="h2" weight="bold">
        {{ $t("pages.account.order_details.title", [order?.number]) }}
      </VcTypography>

      <div class="flex flex-wrap gap-3 print:hidden">
        <VcButton variant="outline" prepend-icon="printer" @click="print()">
          {{ $t("common.buttons.print_order") }}
        </VcButton>

        <VcButton
          v-if="showReorderButton"
          :loading="loadingAddItemsToCart"
          class="min-w-[8.5rem]"
          @click="reorderItems"
        >
          {{ $t("common.buttons.reorder_all") }}
        </VcButton>
      </div>
    </div>

    <VcLayoutWithRightSidebar is-sidebar-sticky>
      <VcWidget id="products" size="lg">
        <!-- Items grouped by Vendor -->
        <div v-if="$cfg.line_items_group_by_vendor_enabled" class="space-y-5 md:space-y-7">
          <template v-for="(group, vendorId) in orderItemsGroupedByVendor" :key="vendorId">
            <div v-if="group.items.length" class="space-y-3">
              <!-- Vendor -->
              <div class="flex max-w-full flex-wrap gap-x-3">
                <VcVendor :vendor="group.vendor" />
                <VcRating v-if="$cfg.rating_enabled && group.vendor?.rating" :rating="group.vendor.rating" />
              </div>

              <OrderLineItems :items="group.items" />
            </div>
          </template>
        </div>

        <!-- Items not grouped by Vendor -->
        <OrderLineItems v-else :items="orderItems" />
      </VcWidget>

      <AcceptedGifts v-if="giftItems.length" :items="giftItems" />

      <OrderCommentSection v-if="order.comment" :comment="order.comment" readonly />

      <template #sidebar>
        <!-- Order Data Widget -->
        <VcWidget :title="$t('common.titles.order_data')" class="order-first">
          <div class="flex flex-col gap-1.5 text-sm">
            <p v-if="order.createdDate">
              <span class="font-extrabold"> {{ $t("common.labels.created") }}: </span>
              {{ $d(order.createdDate) }}
            </p>
            <p v-if="order.status" class="flex items-center">
              <span class="mr-1 font-extrabold"> {{ $t("common.labels.status") }}: </span>
              <OrderStatus class="min-w-[7.785rem] print:min-w-0" :status="order.status" />
            </p>
          </div>
        </VcWidget>

        <!-- Order summary -->
        <OrderSummary
          :cart="order"
          :no-shipping="allItemsAreDigital"
          class="order-last print:order-none print:break-after-page"
        >
          <template #footer>
            <VcButton
              v-if="showPaymentButton"
              class="mt-4"
              full-width
              @click="$router.push({ name: 'OrderPayment', params: { orderId } })"
            >
              {{ $t("common.buttons.pay_now") }}
            </VcButton>
          </template>
        </OrderSummary>

        <!-- Billing Address Widget -->
        <VcWidget v-if="billingAddress" :title="$t('common.titles.billing_address')">
          <VcAddressInfo :address="billingAddress" class="text-15" />
        </VcWidget>

        <!-- Shipping Method Card -->
        <VcWidget v-if="!allItemsAreDigital && shipment" :title="$t('common.titles.shipping_method')">
          <div class="flex items-center gap-4 text-15">
            <VcImage :src="shipment.shippingMethod?.logoUrl" class="h-12 w-12 print:hidden" lazy />

            <span class="min-w-0 break-words">
              {{ $t(`common.methods.delivery_by_id.${shipment.shipmentMethodCode}_${shipment.shipmentMethodOption}`) }}
              ({{ shipment.price?.formattedAmount }})
            </span>
          </div>
        </VcWidget>

        <!-- Shipping Address Card -->
        <VcWidget v-if="!allItemsAreDigital && deliveryAddress" :title="$t('common.titles.shipping_address')">
          <VcAddressInfo :address="deliveryAddress" class="text-15" />
        </VcWidget>

        <!-- Payment Method section -->
        <VcWidget v-if="payment?.paymentMethod" :title="$t('common.titles.payment_method')">
          <div class="flex items-center gap-4 text-15">
            <VcImage :src="payment.paymentMethod.logoUrl" class="h-12 w-12 print:hidden" lazy />
            <span class="min-w-0 break-words">
              {{ $t(`common.methods.payment_by_code.${payment.paymentMethod.code}`) }}
            </span>
          </div>
        </VcWidget>
      </template>
    </VcLayoutWithRightSidebar>
  </div>

  <VcLoaderOverlay v-else no-bg />
</template>

<script setup lang="ts">
import { breakpointsTailwind, useBreakpoints } from "@vueuse/core";
import { computed, ref, watchEffect } from "vue";
import { useI18n } from "vue-i18n";
import { useBreadcrumbs, usePageHead } from "@/core/composables";
import { useUserOrder, OrderLineItems, OrderStatus } from "@/shared/account";
import { AddBulkItemsToCartResultsModal, getItemsForAddBulkItemsToCartResultsPopup, useCart } from "@/shared/cart";
import { AcceptedGifts, OrderCommentSection, OrderSummary } from "@/shared/checkout";
import { BackButtonInHeader } from "@/shared/layout";
import { usePopup } from "@/shared/popup";

interface IProps {
  orderId: string;
}

const props = defineProps<IProps>();

const breakpoints = useBreakpoints(breakpointsTailwind);
const {
  order,
  giftItems,
  orderItems,
  orderItemsGroupedByVendor,
  deliveryAddress,
  billingAddress,
  shipment,
  payment,
  allItemsAreDigital,
  fetchFullOrder,
  clearOrder,
} = useUserOrder();
const { cart, addItemsToCart } = useCart();
const { openPopup } = usePopup();
const { t } = useI18n();

usePageHead({
  title: computed(() => t("pages.account.order_details.meta.title", [order.value?.number])),
});

const breadcrumbs = useBreadcrumbs(() => [
  { title: t("common.links.account"), route: { name: "Account" } },
  { title: t("common.links.orders"), route: { name: "Orders" } },
  { title: t("pages.account.order_details.title", [order.value?.number]) },
]);

const isMobile = breakpoints.smaller("lg");

const loadingAddItemsToCart = ref(false);

const showPaymentButton = computed<boolean>(
  () => !!order.value && (order.value.status === "New" || order.value.status === "Payment required"),
);
const showReorderButton = computed<boolean>(() => !!order.value && order.value.status === "Completed");

async function reorderItems() {
  const items = order.value!.items.filter((item) => !item.isGift);

  loadingAddItemsToCart.value = true;

  await addItemsToCart(items.map(({ productId, quantity }) => ({ productId, quantity })));

  openPopup({
    component: AddBulkItemsToCartResultsModal,
    props: {
      items: getItemsForAddBulkItemsToCartResultsPopup(items, cart.value!),
      listName: t("pages.account.order_details.title", [order.value?.number]),
    },
  });

  loadingAddItemsToCart.value = false;
}

function print() {
  window.print();
}

watchEffect(() => {
  clearOrder();
  void fetchFullOrder({ id: props.orderId });
});
</script>

<style scoped lang="scss">
@media print {
  #products {
    :deep(.vc-section-widget__title) {
      @apply hidden;
    }
  }
}
</style>
